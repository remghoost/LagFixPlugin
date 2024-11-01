// Hello!
// This script has only been tested up through world 2 / stage 2, so I'm not sure if it accounts for everything possible.

using BepInEx;
using BepInEx.Logging;

using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Unity.Mono;
using System.IO;

using System.Collections;
using System.Collections.Generic;

using PerfectRandom.Sulfur.Core.Units;
using System.ComponentModel.Design.Serialization;  // Import for Npc class

namespace LagFixPlugin;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LagFixPlugin : BaseUnityPlugin
{
    private const float DistanceThreshold = 55f;         // Max distance for activation
    private const float CheckInterval = 0.5f;            // Interval for distance checks
    private const float EnemySearchInterval = 5f;        // Interval for refreshing enemy list
    private const float SearchModeInterval = 5f;         // Interval for search mode re-check

    private GameObject player;
    private Transform unitRoot;
    private List<GameObject> enemiesToMonitor = new List<GameObject>();
    private float checkTimer = 0f;
    private float searchTimer = 0f;
    private bool isSearchMode = false;

    private HashSet<GameObject> culledEnemies = new HashSet<GameObject>(); // Track enemies with 'DeadParticles' disabled

    void Start()
    {
        Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} says hi!");
        // Start the plugin in search mode
        EnterSearchMode();
    }

    private void EnterSearchMode()
    {
        Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
        Logger.LogInfo("Entering search mode to locate player and UnitRoot.");
        Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
        isSearchMode = true;

        // Clear lists to avoid memory bloat
        enemiesToMonitor.Clear();
        culledEnemies.Clear();

        StartCoroutine(SearchForGameWorld());
    }

    private IEnumerator SearchForGameWorld()
    {
        while (isSearchMode)
        {
            // Check if player and UnitRoot are available
            player = GameObject.FindWithTag("Player");
            var unitRootObj = GameObject.Find("UnitRoot");

            if (player != null && unitRootObj != null)
            {
                unitRoot = unitRootObj.transform;
                Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                Logger.LogInfo("Game world detected, exiting search mode.");
                Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
                
                // Exit search mode and initialize enemy list
                isSearchMode = false;

                Logger.LogInfo("Enemy monitoring active! ");
                Logger.LogInfo($"-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");

                RefreshEnemiesList();
                yield break;
            }

            yield return new WaitForSeconds(SearchModeInterval); // Retry every few seconds
        }
    }

    void Update()
    {
        // If search mode is active, skip regular update logic
        if (isSearchMode) return;

        // If player or unitRoot is lost, re-enter search mode
        if (player == null || unitRoot == null)
        {
            EnterSearchMode();
            return;
        }

        // Update timers for periodic actions
        checkTimer += Time.deltaTime;
        searchTimer += Time.deltaTime;

        // Perform distance-based toggling on the interval
        if (checkTimer >= CheckInterval)
        {
            checkTimer = 0f;
            ToggleEnemiesBasedOnDistance();
        }

        // Refresh enemy list periodically to capture dynamically spawned enemies
        if (searchTimer >= EnemySearchInterval)
        {
            searchTimer = 0f;
            RefreshEnemiesList();
        }
    }

    private void RefreshEnemiesList()
    {
        enemiesToMonitor.Clear();

        foreach (Transform child in unitRoot)
        {
            // Look for enemies
            // Might need to add more here depending on how enemies work in later levels
            if ((
                child.name.EndsWith("(Clone) (Offensive)") || 
                child.name.EndsWith("(Clone) (Defensive)") || 
                child.name.Contains("(Offensive) [") || 
                child.name.Contains("(Defensive) [")
                ) 

                && !enemiesToMonitor.Contains(child.gameObject))
            {
                enemiesToMonitor.Add(child.gameObject);
            }
        }

        // Logger.LogInfo($"Refreshed enemies list, currently monitoring enemies.");
    }

    private void ToggleEnemiesBasedOnDistance()
    {
        foreach (GameObject enemy in enemiesToMonitor)
        {
            if (enemy == null) continue; // Skip if the enemy has been destroyed or removed

            // Check if the enemy is dead using Npc.IsAlive property
            var npcComponent = enemy.GetComponent<Npc>();
            if (npcComponent == null || npcComponent.IsAlive) continue; // Only proceed if enemy is dead

            float distanceToPlayer = Vector3.Distance(player.transform.position, enemy.transform.position);

            // Find the "Root" child object to manage its active state
            Transform rootTransform = enemy.transform.Find("Root");
            if (rootTransform == null)
            {
                Logger.LogWarning($"No 'Root' found on {enemy.name}. Skipping.");
                continue;
            }

            // Toggle the Root's active state based on distance
            if (distanceToPlayer > DistanceThreshold && rootTransform.gameObject.activeSelf)
            {
                rootTransform.gameObject.SetActive(false);

                // Only disable DeadParticles if this is the first time we're culling this enemy
                if (!culledEnemies.Contains(enemy))
                {
                    Transform deadParticlesTransform = rootTransform.Find("Center/Sprite/DeadParticles");
                    if (deadParticlesTransform != null)
                    {
                        deadParticlesTransform.gameObject.SetActive(false);
                    }
                    culledEnemies.Add(enemy); // Mark this enemy as processed for DeadParticles
                }
            }
            else if (distanceToPlayer <= DistanceThreshold && !rootTransform.gameObject.activeSelf)
            {
                rootTransform.gameObject.SetActive(true);

                // Reset scale to (1, 1, 1) upon activation to prevent size anomalies
                rootTransform.localScale = Vector3.one;
            }
        }
    }

}