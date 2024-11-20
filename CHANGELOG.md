# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [unreleased]

## Fixed

### Games

- Fixed Baldi's basics itch.io throwing exceptions during TAS playback

### UniTAS

- Fixed non-existant axis name checks causing UniTAS to throw exceptions

### Compatibility

- **MAYBE BREAKING**: Setting async operation activation to true won't instantly load, which was the case before which is inaccurate
- AsyncOperation's InvokeCompletionEvent is only disabled for tracked AsyncOperation instances
- Implemented LoadScene forcing pending LoadSceneAsync to be all loaded
- Fixed accidentally obliterating return value of LoadScene instances
- Fixed inaccurate scene load counter

# [v0.6.0] - 2024-11-18

## Added

### UniTAS

- Added object tracker and object picker

### TAS movie

- **maybe breaking**: `MOVIE_CONFIG` now has a `window` table entry. `width` and `height` sets the game window's initial width and height. **1920** x **1080** by default unless specified. You can also set the resolution refresh rate (though it wouldn't matter unless the game is using this information) by setting `refresh_rate` to a number or a table containing `numerator` (or `n`) and `denominator` (or `d`)
  - Additional screen resolutions for the game to pick up on can be added to the list `MOVIE_CONFIG.window.resolutions` (this could be used for changing screen resolutions later on in the menu or something), where each entry of `resolutions` is a table containing `width`, `height`, and `refresh_rate`. Make sure they are valid or it is ignored otherwise
- `movie.no_refresh` function for disabling and enabling game rendering for faster playback

### Compatibility

- Added new input system support, with keyboard and mouse working
- Added basic support for rewired input system support

## Changed

### UniTAS

- Cooler GUI theme
- Most things can be configured within UniTAS GUI

### TAS Movie

- **BREAKING**: Changed `controller` functions, they don't use axis names for controlling anymore for accuracy

### Performance

- Loading should be faster if you've opened the game once with UniTAS
- Slight performance increase in patching the first time

## Fixed

### Games

- ULTRAKILL can be controlled now
- Fixed itch.io Jaded Combat crashing on file selection
- Fixed It Steals not being controllable properly with key, mouse, and controller
- Fixed Lethal Company not loading the ship
- Fixed Lethal Company breaking on soft restart
- Fixed HuniePop soft locking after dialogs due to time being frozen
- Fixed Rain World not booting and being stuck at a black screen
- Fixed Cuphead not loading
- Fixed Baldi's Basics itch.io release from not working

### General

- TAS encoding shouldn't show a command prompt anymore, probably
- Fixed music overlaying on each other every restart
- Fixed UniTAS not limiting realtime FPS making things go super fast

### Compatibility

- Fixed UniTAS not loading at all for games
- Fixed accidentally skipping over static constructors
- Fixed accidentally skipping over removing some readonly fields
- Fixed crash when trying to remove readonly fields
- Fixed sometimes UniTAS not receiving the first update, which might cause some movie desync by 1 frame
- Fixed soft restart not properly taking in account for threads
- Fixed soft restart not resetting cursor visibility and lock state
- Fixed old input system's axis not being controlled by mapped inputs
- Fixed async scene load event callback to be at proper timings
- Fixed AsyncOperation instance not returning proper values
- Fixed invoking RuntimeInitializeOnLoadMethod in the wrong order
- Fixed games using ISerializationCallbackReceiver not receiving OnAfterDeserialize callback on soft restart
- Fixed old games with only Time.captureFramerate appearing to have frozen time
- Fixed most things related to asset bundles
- Fixed old unity game times being frozen (unity 5 games and before most likely was affected)
- Fixed static constructor throwing causing UniTAS to incorrectly track static constructors
- Fixed unity not being reset, related to static extern properties
- Fixed ScriptableObject not being reset to their initial state

# [v0.5.1] - 2023-09-13

## Fixed

### Games

- Fixed Cat Quest not being TASable due to a bug in UniTAS
- Fixed DDLC launch on crash because out of range system time
- Fixed Needy Streamer crashing on start up

### TAS playback
- Fixed timing being offset wrong by whatever value is fixed update is at

### Compatibility

- Fixed some games using switch statement skips over UniTAS tracking the end of static constructors
- Fixed not using the right ILCode for returning a default value from a method
- Fixed duplicate AssetBundle loading causing an exception
- Fixed coroutines causing exception on restart
- Fixed some timing issues

## Changed

### UniTAS

- Stopped setting unity running in background to false (this was causing issues with some games when you tab out)
- UniTAS system time by default is now the current time (until you run a movie)

# [v0.5.0] - 2023-07-04

## Added

### Config

- Added live config reloading
- Added config for binding keys to actions

### Overlays

- Added built in overlays
- Added frame counter overlay
- Added mouse overlay

### TAS movie

- Added `unity` module for interacting with unity engine
- Added `unity.find_objects_by_type()` to find all objects of a type
- Added wrapped type for `UnityEngine.Transform` to read transform data for objects
- Added `gui` module for interacting with UniTAS GUI
- Added `gui.show_overlays` to show or hide overlays

### UniTAS

- Added built in overlay GUIs for TAS information

## Fixed

### UniTAS

- Fixed movie playback fps unlock not working
- Fixed movie running while game is paused while tabbed out

### Games

- Fixed deepest sword crashing randomly when running a TAS

### Compatibility

- Fixed readonly fields pointing to old references after soft restart
- Stop threads created by game from running after soft restart
- Prevent finalizers from running during soft restart
- Fixed some static constructors not being tracked properly
- Added calls to RuntimeInitializeOnLoadMethod attribute methods

## Changes

### UniTAS

- Now you can run a different TAS while one is already running

### Performance

- Improved performance of changing fps

### GUI

- Updated GUI to new style
- Added terminal
- Added ability to move and resize windows
- Added movie file browser

## Security

### TAS Movie

- Sandboxed lua environment for TAS movie playback

# [v0.4.0] - 2023-06-02

## Added

### TAS movie

- Added config `MOVIE_CONFIG.update_type` and API `env.update_type` to choose what update frame advancing should use

### New input system support

- Added *very basic* new input system support
- Added support for keyboard and mouse devices

### Old input system support

- Added support for `ResetInputAxes`
- Added support for `mouseScrollDelta`

### Testing

- Added runtime unit tests to test if UniTAS features works

## Fixed

### TAS movie

- Fixed accidentally skipping a frame when an unity `FixedUpdate` happens

### Compatibility

- Fixed AssetBundle async operations failing with null reference exception
- Fixed game setting AsyncOperation priority throwing an exception
- Fixed unity's Camera events not being cleared on soft restart throwing a null reference exception
- Fixed unity's SceneManager events not being cleared on soft restart throwing a null reference exception
- Fixed unity's NavMesh events not being cleared on soft restart throwing a null reference exception
- Fixed opened files not being closed on soft restart, causing IO exceptions
- Fixed static constructor dependency called in the wrong order
- Fixed static fields not being reset all at once
- Fixed not being able to call some async methods in AssetBundle

### UniTAS internal
- Fixed UniTAS not patching AsyncOperation classes if SceneManagerAPIInternal isn't found
- Fixed UniTAS's wrapped classes not handling the case of the original wrapped type being null
- Fixed not resolving monobehaviour types more than 1 parent
- Fixed use of unload scene being called wrong causing null reference exception
- Fixed not searching async load method in scene manager
- Fixed AsyncOperation's OnComplete being not called at the right timing

# [v0.3.0] - 2023-04-10

## Added

### TAS movie

- Added `movie` module
- Added `movie.playback_speed()` to set playback speed
- Added FPS unlocking for TAS playback
- Added video rendering with `movie.start_capture()` and `movie.stop_capture()`

## Changed

### TAS movie
- Errors at runtime and set up now don't dump the exception stack trace
- Renamed and moved method `adv` as `movie.frame_advance`
- Allow the user to set the RNG seed at start up with `MOVIE_CONFIG.seed`
- Set TAS playback speed to be 0 unless user changes it

### UniTAS set up tool
- Moved UniTAS set up tool to [unitas-setup-tool](https://github.com/Eddio0141/unitas_setup_tool)

## Fixed

### TAS movie

- Fixed movie not playing again if a runtime error occurs
- Fixed movie not playing again if error occurs when parsed
- Fixed movie respecting the FPS limit if game has a fixed FPS
- Fixed accidentally invoking engine methods while set up

### TAS playback

- Fixed time not getting the same value every TAS playback
- Fixed Time.timeScale not resetting on soft restart

### Compatibility

- Fixed not being able to patch class deriving MonoBehaviour that has generic parameters for pausing and accurately
  updating TAS input
- Fixed unity time being wrong
- Prevent an extra update from being called on the first frame of soft restart

## Removed

### TAS movie

- Removed `MOVIE_CONFIG.ft` since `MOVIE_CONFIG.frametime` already exists

# [v0.2.0_v1.1.3] - 2023-03-02

## Changed

### TAS movie

- Now uses lua 5.2, check the new [wiki](https://github.com/Eddio0141/UniTAS/wiki) for tutorials and documentation

### UniTAS GUI

- Temporarily removed unused buttons until they are implemented

### UniTAS set up tool
- Updated dependencies

# [v0.1.0_v1.1.2] - 2023-02-24

## Fixed

### UniTAS set up tool
- Fixed installing UniTAS from GitHub release

# [v0.1.0_v1.1.1] - 2023-02-24

## Added

### Basic movie playback
- Added script language for TAS movies
- Ability to soft restart game for TAS playback

### Game general support
- Able to control games using legacy input system
- Support for async scene loading
- Soft restart destroys all DontDestroyOnLoad game objects
- Game's static fields are detected and reset on soft restarting
- Override system time
- Able to set game FPS to be fixed for movie playback

### UniTAS GUI

- Basic GUI for playback

### UniTAS set up tool

- Able to download UniTAS stable, nightly, and by tag
- Able to download BepInEx stable, nightly, and by tag
- Able to set up BepInEx and UniTAS to an unity game directory
- Game directory access history and usage

[unreleased]: https://github.com/Eddio0141/UniTAS/compare/v0.6.0...HEAD
[v0.6.0]: https://github.com/Eddio0141/UniTAS/compare/v0.5.1...v0.6.0
[v0.5.1]: https://github.com/Eddio0141/UniTAS/compare/v0.5.0...v0.5.1
[v0.5.0]: https://github.com/Eddio0141/UniTAS/compare/v0.4.0...v0.5.0
[v0.4.0]: https://github.com/Eddio0141/UniTAS/compare/v0.3.0...v0.4.0
[v0.3.0]: https://github.com/Eddio0141/UniTAS/compare/v0.2.0_v1.1.3...v0.3.0
[v0.2.0_v1.1.3]: https://github.com/Eddio0141/UniTAS/compare/v0.1.0_v1.1.2...v0.2.0_v1.1.3
[v0.1.0_v1.1.2]: https://github.com/Eddio0141/UniTAS/compare/v0.1.0_v1.1.1...v0.1.0_v1.1.2
[v0.1.0_v1.1.1]: https://github.com/Eddio0141/UniTAS/releases/tag/v0.1.0_v1.1.1
