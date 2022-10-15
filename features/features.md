# Essential features
- TAS movie replay (from game start)
- [TAS movie input scripting](movie.md)
- Movie rendering
- In-game menu
- TAS tool installer / uninstaller

# Non essential features
- Save states
- In-game xray
- TAS playback slowdown
- Movie generator (e.g. for bruteforcer implementation)

# Feature notes
- TAS movie replay
  - Menu to choose what TAS to replay with
  - Set virtual environment and simulate game start
  - Retrieve actions from TAS movie and apply to game
  - Movie end, let user be able to control the game again

- Movie rendering
  - Movie can call a method to start / stop rendering
  - `record(path_to_mp4) stop_record()`

- TAS tool installer / uninstaller
  - Install on unity game
    - Checks if dir contains an unity game
    - Checks if correct BepInEx is already installed
      - If installed, check version
        - If version correct, check if TAS Plugin and Patcher is installed
          - If incorrect, install latest plugin and patcher
        - If version incorrect, error message "BepInEx is already installed with the incorrect version"
      - If not installed
        - Install bepinex
        - Run game once
        - Exit game
        - Configure bepinex
        - Install TAS tool
  - Uninstall
    - Checks if dir is unity game
      - Checks if TAS plugin is installed, remove
      - If other plugins / patchers exist
        - If it exists, inform user about existing BepInEx, exit
      - If no other plugins / patchers exist, remove BepInEx components

- In-game menu
  - TAS play back
    - Browse / input path to movie
    - Run button

- Save states
  - Containing info
    - Environment settings
    - Date time
    - RNG state
    - File state
    - Loaded scenes
    - Loaded game objects
      - Script states
    - Static field states
  - Coroutine problem
    - If coroutine is running then there might not be a way to "save" that information
      - If no way to store coroutine states then I have warn the user that coroutines are running
  - Threading problem
    - If a thread is running then there might not be a way to "save" that information
      - If no way to store coroutine states then I have warn the user that other threads are running

- Movie generator
  - Input script from the user
  - Start from save state / game start
  - Script returns
    - Goals
    - Constraints
  - Script is given
    - Unity variables
    - All game object information
  - Outputs a movie when user finishes generation

- TAS playback slowdown
  - Movie method to slow down playback speed at points in movie
  - `playback_speed(1)` slows game down to be around realtime
  - `playback_speed(0)` removes slowdown
  - `playback_speed(2)` slows game down 2 times

- In-game xray
  - Movie can enable xray with methods
  - `xray(true) xray(false)`
  - TAS menu can access the option

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