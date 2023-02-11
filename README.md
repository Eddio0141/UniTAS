# UniTAS
A tool that lets you TAS unity games

- The tool doesn't bypass anti cheat or anything, USE AT YOUR OWN RISK!
- The tool is early in development and only basic games work
- At its core, this is a [BepInEx 5] plugin

# What games work
- Currently, anything that [BepInEx 5] supports, ranging from unity 3 to latest, and games that use Mono and not IL2CPP
- Check [working-games](docs/working-games.md) for tested games

# Set up tool
- There's a tool that you can use to automatically download and install BepInEx and UniTAS to an unity directory
- Very basic and lacks many features right now
- Tool doesn't have a GUI so you have to use the command line to install BepInEx 

## Usage
### Help
- `unitas_setup_tool --help`
- This is where you wanna look for usage mostly
- You can check command usage with `--help` as well like so: `unitas_setup_tool install --help`

### Installing stable
`unitas_setup_tool install <GAME_DIR_SELECTION>`

### Installing nightly builds of BepInEx / UniTAS
`unitas_setup_tool install --branch <BRANCH> --bepinex-branch <BRANCH>`

# How to build
- Make sure you have [.NET SDK 6.0 or 7.0](https://dotnet.microsoft.com/en-us/download) and [rust](https://www.rust-lang.org/tools/install) installed on your system
- Run either `build.sh` for linux or `build.ps1` for windows
  - Takes either `Debug` or `Release` as an argument for choosing building config
  - `ReleaseTrace` builds with a `Release` profile but with trace logging for the plugin
  - Passing in nothing will build `Debug` automatically
- Check `build` directory for built files

[BepInEx 5]: https://docs.bepinex.dev/articles/user_guide/installation/index.html
