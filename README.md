# UniTAS
A tool that lets you TAS unity games hopefully

# <span style="color:red">!!!The tool doesn't bypass anti cheat or anything like that, USE AT YOUR OWN RISK!!!</span>

# Stuff you might want to know
- The tool is still in a very early stage
- It only does very basic stuff, mainly that being running a TAS consistently
- It still has no GUI for control
- The only code right now is in Plugin which is a [BepInEx](https://docs.bepinex.dev/master/) plugin, and only tested with the game "It Steals" latest version
- Has many testing code that won't work on other pcs such as:
  - Building the UniTASPlugin plugin would try copy the dll to `C:\Program Files (x86)\Steam\steamapps\common\It Steals\BepInEx\plugins`
  - Plugin runs only with netstandard2.1
- Pressing "K" would run a test TAS from path `C:\Program Files (x86)\Steam\steamapps\common\It Steals\test.uti`
- Currently no convenient tool that installs this TAS tool to some unity game
- Only tested in windows

# TAS tool features
- [x] TAS play back
- [ ] Ingame TAS recording
- [ ] Savestates
- [ ] Game video recording
  - Only can dump raw images to disk 
- [ ] TAS menu
- [ ] TAS GUI
- [ ] TAS helpers
  - [ ] Get all axis names in input legacy system
    - Only prints them in console when found
- [ ] Frame advance / slow down
- [ ] Optional resolution

# Known bugs
- Different scene possibly desyncing while heavy load on Plugin.Update / Plugin.FixedUpdate (maybe use a coroutine)
### It Steals
- The loading text is different from when you run a TAS with soft restart from in game, and from main menu
- the game's play button breaks when you soft restart while waiting for next scene to load

# VR Support
I haven't planned for VR support currently

# L2CPP Support
Depends on BepInEx's progress on it

# Important TODOs
- Separate tool to set up the TAS tool for a unity game
- Integrate BepInEx to project
- Build script or something to build everything properly
- A way to handle additional unity patches which is a dependency not included by default
- TAS GUI
- Template for unity version
- Check if I need netstandard2.1
- Each unity version might need to build for netstandard2.0, net46, net35. [read this](https://docs.microsoft.com/en-us/nuget/create-packages/multiple-target-frameworks-project-file).
- What to do with additional unity dependencies? Currently 2021.2.14 has UnityEngine.UI required but maybe can get away with trying to bind
- Not sure if different net versions needs to be built and supported for each unity version support
- Not sure if UnityEngine.UI exists in other games with same game version

# Version support
- 2021.2.14
  - [x] .NET Standard 2.1
  - [x] .NET Standard 2.0
  - [ ] .NET Framework 4.6
  - [ ] .NET Framework 3.5
- 2018.4.25
  - [ ] .NET Standard 2.0
  - [ ] .NET Framework 4.6
  - [x] .NET Framework 3.5

# Supported games
- "It Steals"
  - 12.4

# Adding unity version support
// TODO create template
// Some information about UnityHelpers binding

# Adding patches
- If the patch target exists across all versions of .NET / Unity in the same form, add to Core/Patches

# Background tasks to be finished
- Update() and FixedUpdate() calls in core needs to be done before Unity calls happen, hook to make it work.
- Create core patches which will be patches for things that exists in all unity versions / .NET versions
- Pass assembly information to some initializer for Core.UnityHooks and use reflection to get the types rather than finding each type in Plugin initialization
- Clean up Plugin and UnityASyncHandler type passing
- Full input legacy system override
  - [x] Mouse clicks
  - [x] Axis & value control
  - [ ] Button presses
  - [ ] find out what the difference between GetAxis and GetAxisRaw is
  - [ ] Mouse movement
    - [ ] get_mousePosition_Injected set `ret`
    - Has some mouse movement, UI works at the very least
  - [ ] Mouse scrolling
  - [ ] UI control
  - [ ] Keyboard presses
    - KeyCode works but not overriding string variant of GetKey checks and not supported in keyboard system
  - [ ] Touch screen
  - [ ] GetAccelerationEvent call
  - [ ] simulateMouseWithTouches call
  - [ ] imeCompositionMode call
  - [ ] compositionCursorPos call
  - [ ] location getter purpose
  - [ ] CheckDisabled purpose
  - [ ] What to do with setters in module
  - [ ] Other devices
- Full new input system override
- Game capture
  - [ ] Audio recording
  - [ ] Faster video recording
- Disable network
- Soft restart needs to reset save files
- Savestates
  - [ ] Save
    - [ ] Save current scene info
    - [ ] Save graphics info
    - [ ] Save object IDs
    - [ ] Save object states
    - [ ] Save system time
    - [ ] Save game files
    - [ ] Wait for FixedUpdate or count current FixedUpdate iteration
    - [ ] Find other game states
  - [ ] Load
    - [ ] Load scene if not on the correct one
    - [ ] Load missing objects
    - [ ] Unload objects not in save
    - [ ] Load object states
    - [ ] Set system time
    - [ ] Load game files
- Resolution needs to be defined in movie
- DateTime customizability in movie and seed will use that type too
- Time.captureDeltaTime needs to be unable to be changed by user while movie is running
- Movie file input macro functions
- Movie file TAS helper function calls
- Movie end notification on screen (very important)
- Movie frame count on screen (also very important and funny)
- New Framebulk instance not warning or throwing with FrameCount being 0 or too high than int max
- Fix virtual cursor
- Virtual cursor needs to have default texture
- Objects like Plugin and UnityASyncHandler needs to be made sure to not be destroyed or cloned
- Brute forcer stuff
- Lua and other scripting methods?
- System.Random
  - [ ] System.Random.GenerateSeed check if consistent generation
  - [ ] System.Random.GenerateGlobalSeed check if consistent generation
- Movie needs to store additional information of recorded pc such as whats in CultureInfo
- Movie matching unity version and checks for that version such as keycode
- Movie matching game name?
- Movie store game version
- Movie ability to switch between captureFramerate and captureDeltaTime
- Input legacy system KeyCode
  - [ ] 3.5.1-3.5.5: has everything except below
  - [ ] 4.0-4.6: adds LeftCommand, RightCommand
  - [ ] 5.0-2018.2: adds Joystick5Button0 all the way to Joystick8Button19
  - [ ] 2018.3-2021.1: adds Percent, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde
  - [x] 2021.2-2022.2: adds LeftMeta, RightMeta
  - [ ] 2023.1: adds WheelUp, WheelDown
- Movie can set window focus