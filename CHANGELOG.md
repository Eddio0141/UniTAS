# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [0.1.0] - 2023-02-24

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

[0.1.0]: https://github.com/Eddio0141/UniTAS/releases/tag/v0.1.0