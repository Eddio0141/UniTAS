# UniTAS
A tool that lets you TAS unity games hopefully

# !!!The tool doesn't bypass anti cheat or anything related, USE AT YOUR OWN RISK!!!

# The code is being refactored: [branch](https://github.com/Eddio0141/UniTAS/tree/Eddio0141/issue62), [issue](https://github.com/Eddio0141/UniTAS/issues/62).

# Stuff you might want to know
- The tool is still in a very early stage
- Only code that exists right now is in /Plugin and /Patcher, which you have to use along with [bepinex 5](https://docs.bepinex.dev/articles/user_guide/installation/index.html), place the built dll of /Plugin in the bepinex plugin folder, and /Patcher dll into /Patch
- It only does very basic stuff, mainly that being running a TAS consistently
- Currently no convenient tool that installs this TAS tool to some unity game
- Only tested in windows

# VR Support
I haven't planned for VR support currently

# Supporting unity versions
For now, anything that BepInEx 5.4.21 can support, ranging from unity 3 to latest, and games that don't use Il2cpp

# Adding patches for unity and .NET framework
- All patches goes in plugin's Patches folder
- __namespace for each namespace of the patch method, e.g. patch for UnityEngine.EventSystems.EventSystem.isFocused, we will put the patch class `isFocusedGetter` in `__UnityEngine/EventSystems/EventSystem.cs`
- Make sure to separate each patch as a separate patch class, This prevents all patches in the patch class from failing if 1 fails
- Use `static Exception Cleanup` method in patch class and use the helper methods depending on situations as below does:
```cs
// this will simply prevent the method from being patched if it doesn't exist
static System.Exception Cleanup(MethodBase original, System.Exception ex)
{
    return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
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

# Background tasks to be finished (move to issues)
- Update() and FixedUpdate() calls in core needs to be done before Unity calls happen, hook to make it work.
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
- Plugin needs to be made sure to not be destroyed or cloned
