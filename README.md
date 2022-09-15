# UniTAS
A tool that lets you TAS unity games hopefully

# <span style="color:red">!!!The tool doesn't bypass anti cheat or anything like that, USE AT YOUR OWN RISK!!!</span>

# Stuff you might want to know
- The tool is still in a very early stage
- It only does very basic stuff, mainly that being running a TAS consistently
- It still has no GUI for control
- The only code right now is in UniTASPlugin which is a [BepInEx](https://docs.bepinex.dev/master/) plugin, and only tested with the game "It Steals" latest version
- Has many testing code that won't work on other pcs such as:
  - Building the UniTASPlugin plugin would try copy the dll to `C:\Program Files (x86)\Steam\steamapps\common\It Steals\BepInEx\plugins`
  - Relies on dependency for `UnityEngine.UI.dll` for testing from path `Program Files (x86)\Steam\steamapps\common\It Steals\It Steals_Data\Managed\UnityEngine.UI.dll`
  - Plugin runs only with netstandard2.1
  - Plugin was never tested in 32 bits
- Pressing "K" would run a test TAS from path `C:\Program Files (x86)\Steam\steamapps\common\It Steals\test.uti`
- Currently no way of supporting other unity versions or a convenient tool that installs this TAS tool to some unity game
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

# Background tasks to be finished
- [x] RNG and time problems
- [x] No inconsistent scene loads
- [ ] Full input legacy system override
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
  - [ ] Other devices
- [ ] Full new input system override
- [ ] Game capture
  - [ ] Audio recording
  - [ ] Faster video recording
- [ ] Resolution needs to be defined in movie
- [ ] DateTime customizability in movie and seed will use that type too
- [ ] Time.captureDeltaTime needs to be unable to be changed by user while movie is running
- [ ] Movie file input macro functions
- [ ] Movie file TAS helper function calls
- [ ] Movie end notification on screen (very important)
- [ ] Movie frame count on screen (also very important and funny)
- [ ] New Framebulk instance not warning or throwing with FrameCount being 0 or too high than int max
- [ ] Fix virtual cursor
- [ ] Virtual cursor needs to have default texture
- [ ] Objects like Plugin and UnityASyncHandler needs to be made sure to not be destroyed or cloned
- [ ] Brute forcer stuff
- [ ] Lua and other scripting methods?

# Known bugs
- Different scene possibly desyncing while heavy load on Plugin.Update / Plugin.FixedUpdate (maybe use a coroutine)
### It Steals
- The loading text is different from when you run a TAS with soft restart from in game, and from main menu
- the game's play button breaks when you soft restart while waiting for next scene to load