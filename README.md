# UniTAS
A tool that lets you TAS unity games

# The tool doesn't bypass anti cheat or anything, use at your own risk

# What games work
Check [working-games](docs/working-games.md) for tested games

# Stuff you might want to know
- The tool is still WIP, and is being heavily developed
- Parts of the code could be getting refactored for the sake of cleaning up trash I wrote, usually those file / folder names have `Legacy` on them
- This is a [bepinex 5](https://docs.bepinex.dev/articles/user_guide/installation/index.html) plugin / patch
- It can run TAS consistently, but still have many issues that needs to be resolved to run on big games
- Currently no convenient tool that installs this TAS tool to some unity game, you have to manually install BepInEx for your unity game and drag all the dlls in the right place

# Supporting unity versions
For now, anything that BepInEx 5.4.21 can support, ranging from unity 3 to latest, and games that use Mono and not Il2cpp

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
- Make sure you have [.NET SDK 6.0](https://dotnet.microsoft.com/en-us/download) and [rust](https://www.rust-lang.org/tools/install) installed on your system
- Run either `build.sh` for linux or `build.ps1` for windows
  - Takes either `Debug` or `Release` as an argument for choosing building config
  - `ReleaseTrace` builds with a `Release` profile but with trace logging for the plugin
  - Passing in nothing will build `Debug` automatically
- Check `build` directory for built files