# UniTAS
[![Discord](https://img.shields.io/discord/1093033615161573490)](https://discord.gg/ddMqdqgPeB)

A tool that lets you TAS unity games

- The tool doesn't bypass anti cheat or anything, USE AT YOUR OWN RISK!
- :warning: The tool is early in development and only basic games work
    - This also means TASes made in earlier versions might not work in later versions
- This is a [BepInEx 5] patcher

# What games work

- Currently, anything that [BepInEx 5] supports, ranging from unity 3 to latest, and games that use Mono and not IL2CPP
- Check [compatibility-list](docs/compatibility-list.md) for tested games
- Currently, simple games that comes from game jam or small indie games like on itch.io has a higher chance of working

# How to install

## Manual

- Install [BepInEx 5] to your game
- Download the latest release from [here](https://github.com/Eddio0141/UniTAS/releases/latest)
- Move the `patchers` folder into your game's `BepInEx` folder

## Set up tool

You can use [unitas_setup_tool](https://github.com/Eddio0141/unitas_setup_tool) to set up UniTAS and BepInEx for you

# Trouble shooting and useful information

Please check out the [wiki](https://github.com/Eddio0141/UniTAS/wiki)!

# How to build

- Make sure you have [.NET SDK 6.0 or 7.0](https://dotnet.microsoft.com/en-us/download) installed on your system
- Run either `build.sh` for linux or `build.ps1` for windows
    - Takes either `Debug` or `Release` as an argument for choosing building config
    - `ReleaseTrace` builds with a `Release` profile but with trace logging for the plugin
    - Passing in nothing will build `Debug` automatically
- Check `build` directory for built files

[BepInEx 5]: https://docs.bepinex.dev/articles/user_guide/installation/index.html
