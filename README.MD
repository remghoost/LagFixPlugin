# LagFixPlugin for SULFUR 0.9.0

**This mod looks for dead enemies and deactivates them when they're far enough away from the player.**

I've found that it drastically reduces lag in stages with 10+ dead enemies.

**If the game's developer is reading this, please feel free to steal this code and implement it in-game! (I'd like some credit though... haha)**

---

## Requirements and setup

It's pretty bog-standard bepinex Unity modding, but we'll run through the setup just in case you're new to this.

1. Grab `BepInEx-Unity.Mono-win-x64-6.0.0-pre.2.zip` from the [releases section of their github](https://github.com/BepInEx/BepInEx/releases).
2. Extract that into the game's root folder.
3. Run the game once to generate the necessary folders.
4. Download the mod `.zip` from [the releases section of this github repo](https://github.com/remghoost/LagFixPlugin/releases/).
5. Extract `LagFixPlugin` into `SULFUR/BepInEx/Plugins`.
6. Done!

---
## Building from source

If you're not comfortable downloading a random `.dll` from a stranger on the internet (which, I don't blame you), you can build this plugin from source.

1. `git clone` this repo.
2. Retarget `PerfectRandom.Sulfur.Core.dll` in `LagFixPlugin.csproj` to wherever your game exists.
3. `dotnet build`

---
## Considerations
- Shoutout to ChatGPT for always being a homie
- I probably didn't get every edge case (though I got a bunch of them). 
- Submit an issue if you find something wonky.
- **This mod might break some things (it shouldn't though) and comes with no warranty!**