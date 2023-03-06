# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [Unreleased]

## Added

### TAS movie
- Added `movie` module
- Added `movie.playback_speed()` to set playback speed
- Added FPS unlocking for TAS playback
- Added video rendering with `movie.start_capture()` and `movie.stop_capture()`

## Changed

### TAS movie
- Errors at runtime and set up now don't dump the exception stack trace
- Renamed method `adv` to `frame_advance`

## Fixed

### TAS movie
- Fixed movie not playing again if a runtime error occurs
- Fixed movie not playing again if error occurs when parsed
- Fixed movie respecting the FPS limit if game has a fixed FPS

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

[unreleased]: https://github.com/Eddio0141/UniTAS/compare/v0.2.0_v1.1.3...HEAD
[v0.2.0_v1.1.3]: https://github.com/Eddio0141/UniTAS/compare/v0.1.0_v1.1.2...v0.2.0_v1.1.3
[v0.1.0_v1.1.2]: https://github.com/Eddio0141/UniTAS/compare/v0.1.0_v1.1.1...v0.1.0_v1.1.2
[v0.1.0_v1.1.1]: https://github.com/Eddio0141/UniTAS/releases/tag/v0.1.0_v1.1.1