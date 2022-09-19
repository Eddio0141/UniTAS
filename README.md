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
- the game's play button breaks when you soft restart while waiting for next scene to load

# VR Support
I haven't planned for VR support currently

# L2CPP Support
Depends on BepInEx's progress on it

# Important TODOs
- Use Harmony's Traverse utilities for all reflection stuff
- Separate tool to set up the TAS tool for a unity game
- Integrate BepInEx to project
- Build script or something to build everything properly
- TAS GUI
- Only use types and methods that exist across all supported unity versions with same args (or at least use object as input or something)
- Patch types by detecting if they exist and ignore if it doesn't

# Working versions
- 2021.2.14
- 2018.4.25

# Supported games
- "It Steals"
  - 12.4

# Adding patches for some unity version
- All patches goes in plugin's Patches folder
- Depending on the purpose of the patch, create or use an existing folder for it, e.g. TASInput if overriding some new Input function for the TAS inputs
- Make sure to separate each patch as a separate patch class, This prevents all patches in the patch class from failing if 1 fails
- Use `static Exception Cleanup` method in patch class and use the helper methods depending on situations as below does:
```cs
// if the patch exists on some unity version or not (this will simply prevent the method from being patched)
static System.Exception Cleanup(MethodBase original, System.Exception ex)
{
    return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
}

// -------------------------------------------------------------------------

// if the patch NEEDS to be patched (this will show an error in the console, but will let the TAS tool continue anyway)
static System.Exception Cleanup(System.Reflection.MethodBase original, System.Exception ex)
{
    return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
}
```
- If the patch fails even if the method exists in the game, you should use the method `static MethodBase TargetMethod()` in the patch class. Example below:
```cs
[HarmonyPatch]
class UnloadSceneAsync
{
    // I recommend using AccessTools helper to find the method
    static MethodBase TargetMethod()
    {
        var sceneManagerType = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");
        return AccessTools.Method(sceneManagerType, "UnloadSceneAsync", new Type[] { typeof(int) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(int sceneBuildIndex, ref AsyncOperation __result)
    {
        // some patch code
    }
}
```
- How to know if the patch works? Check debug output of the plugin by enabling debug print through `GAME_DIR\BepInEx\config\BepInEx.cfg`, field `[Logging.Disk] LogLevel` or `[Logging.Console] Loglevel` and it will show all methods that failed to patch in the `GAME_DIR\BepInEx\LogOutput.log` or the console

# Background tasks to be finished
- Check whats in SceneManagerAPI, do they need to be patched too
- SystemInfo.supportsGyroscope needs to be patched, UnityEngine.SystemInfo needs to be checked for patches
- Check InputUnsafeUtility and patch them in unity versions that has them
- Update() and FixedUpdate() calls in core needs to be done before Unity calls happen, hook to make it work.
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
    - [x] Save current scene info
    - [ ] Save graphics info
    - [ ] Save object IDs
    - [ ] Save object states
    - [x] Save system time
    - [ ] Save game files
    - [ ] Wait for FixedUpdate or count current FixedUpdate iteration
    - [ ] Find other game states
  - [ ] Load
    - [x] Load scene if not on the correct one
    - [ ] Load missing objects
    - [ ] Unload objects not in save
    - [ ] Load object states
    - [x] Set system time
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
- Objects like Plugin and UnityASyncHandler needs to be made sure to not be destroyed or cloned (BepInEx.cfg can fix this problem most likely)
- Brute forcer
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

# Notes for myself
```
what doesn't exist in modern UnityEngine.Input but does in old
mainGyroIndex_Internal
GetRotation
GetPosition

what doesn't exist in old UnityEngine.Input but does in modern
GetLastPenContactEvent
ResetPenEvents
ClearLastPenContactEvent
SimulateTouch
SimulateTouchInternal
simulateMouseWithTouches
mouseScrollDelta
imeCompositionMode
compositionString
imeIsSelected
compositionCursorPos
touchPressureSupported
stylusTouchSupported
touchSupported
compensateSensors
location
compass
GetGyroInternal
GetTouch_Injected
GetLastPenContactEvent_Injected
GetAccelerationEvent_Injected
SimulateTouchInternal_Injected
get_mousePosition_Injected
get_mouseScrollDelta_Injected
get_compositionCursorPos_Injected
set_compositionCursorPos_Injected
get_acceleration_Injected

what moved to InputUnsafeUtility in modern from old
GetKeyUpString
GetKeyString
GetKeyDownString

what arguments changed from old to modern UnityEngine.Input
GetKeyInt
GetKeyUpInt
GetKeyDownInt

notes from old to new
isGyroAvailable deprecated, use SystemInfo.supportsGyroscope instead
```