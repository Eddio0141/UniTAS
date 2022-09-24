# UniTAS
A tool that lets you TAS unity games hopefully

# <span style="color:red">!!!The tool doesn't bypass anti cheat or anything like that, USE AT YOUR OWN RISK!!!</span>

# Stuff you might want to know
- The tool is still in a very early stage
- It only does very basic stuff, mainly that being running a TAS consistently
- It still has no GUI for control
- Pressing "K" would try run a test TAS
- Pressing "L" would try run a test code (most likely disconnect your input or throw an exception)
- Currently no convenient tool that installs this TAS tool to some unity game
- Only tested in windows

# TAS tool features
- TAS play back

# Known bugs
- Different scene possibly desyncing while heavy load on Plugin.Update / Plugin.FixedUpdate (maybe use a coroutine)
### Cat Quest
- Soft restart causes an exception

# VR Support
I haven't planned for VR support currently

# L2CPP Support
Depends on BepInEx's progress on it

# Important TODOs
- Support of new input system (UnityEngine.InputSystem)
- Soft restart needs to reset game state
- Savestates

# Working versions
- 2019.4.16

# Supported games
- "It Steals"
  - 12.4

# Adding patches for some unity version
- All patches goes in plugin's Patches folder
- __namespace for each namespace of the patch method, e.g. patch for UnityEngine.EventSystems.EventSystem.isFocused, we will put the patch class `isFocusedGetter` in `__UnityEngine/EventSystems/EventSystem.cs`
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
- Separate tool to set up the TAS tool for a unity game
- Integrate BepInEx to project
- Build script or something to build everything properly
- Movie sets screen res and force Screen.width height getters to return the patched value, but if game calls SetResolution then internal state sets to that
- Check whats in SceneManagerAPI, do they need to be patched too
- SystemInfo.supportsGyroscope needs to be patched, UnityEngine.SystemInfo needs to be checked for patches
- Check InputUnsafeUtility and patch them in unity versions that has them
- Update() and FixedUpdate() calls in core needs to be done before Unity calls happen, hook to make it work.
- Ingame TAS recording
- Savestates
- Game video recording
  - Only can dump raw images to disk 
- TAS menu
- TAS GUI
- TAS helpers
  - Get all axis names in input legacy system
    - Only prints them in console when found
- Frame advance / slow down
- Optional resolution
- Full input legacy system override
  - [x] Mouse clicks
  - [x] Axis & value control
  - [ ] Button presses
  - [ ] find out what the difference between GetAxis and GetAxisRaw is
  - [ ] Mouse movement
    - [x] get_mousePosition_Injected set `ret`
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
- Movie file input macro functions
- Movie file TAS helper function calls
- Movie end notification on screen (very important)
- Movie frame count on screen (also very important and funny)
- New Framebulk instance not warning or throwing with FrameCount being 0 or too high than int max
- Fix virtual cursor
- Plugin needs to be made sure to not be destroyed or cloned
- Brute forcer
- Lua and other scripting methods?
- Movie needs to store additional information of recorded pc such as whats in CultureInfo
- Movie matching unity version and checks for that version such as keycode
- Movie matching game name?
- Movie store game version
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