# UniTAS
A tool that lets you TAS unity games

# The tool doesn't bypass anti cheat or anything, use at your own risk

# Stuff you might want to know
- The tool is still in a very early stage
- Parts of the code could be getting refactored for the sake of cleaning up trash I wrote, usually those file / folder names have `Legacy` on them
- This is a [bepinex 5](https://docs.bepinex.dev/articles/user_guide/installation/index.html) plugin / patch
- It can run TAS consistently, but still have many issues that needs to be resolved to run on big games
- Currently no convenient tool that installs this TAS tool to some unity game, you have to manually install BepInEx for your unity game and drag all the dlls in the right place

# How to build
- Make sure you have .NET SDK 6.0 installed on your system
- Run either `build.sh` for linux or `build.ps1` for windows
  - Takes either `Debug` or `Release` as an argument for choosing building config
  - Passing in nothing will build `Debug` automatically
- Check `build/Debug` or `build/Release` for built files

# VR Support
No plans now

# Supporting unity versions
For now, anything that BepInEx 5.4.21 can support, ranging from unity 3 to latest, and games that use Mono and not Il2cpp

- How to know if the patch works? Check debug output of the plugin by enabling debug print through `GAME_DIR\BepInEx\config\BepInEx.cfg`, field `[Logging.Disk] LogLevel` or `[Logging.Console] Loglevel` and it will show all methods that failed to patch in the `GAME_DIR\BepInEx\LogOutput.log` or the console