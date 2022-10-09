# Essential features
- TAS movie replay (from game start)
- TAS movie input scripting
- Movie rendering
- In-game menu
- TAS tool installer / uninstaller

# Non essential features
- Save states
- In-game xray
- TAS movie macros
- TAS movie helper methods
- TAS playback slowdown
- Movie generator (e.g. for bruteforcer implementation)

# Feature notes
- TAS movie replay
  - Menu to choose what TAS to replay with
  - Set virtual environment and simulate game start
  - Retrieve actions from TAS movie and apply to game
  - Movie end, let user be able to control the game again

- TAS movie script structure
  - Comments: `// comment`
  - Movie properties
    - Virtual environment
      - From startup (startup flag)
        - Operating system
        - Date time / seed to start with
        - Frametime or FPS
      - From save state (state flag with path to save state)
        - Load environment settings from state
    - Name
    - Description
    - Author
    - End save
      - Creates a save state at end for using it on another movie
  - User defined movie methods
    - Name (unique)
    - Arguments
    - Can return something
    ```
    // example code
    fn do_some_actions(arg1, arg2) {
        // would press A in the first frame and press B in the next
        key(A)
        key(B)
        // use of arguments
        Mouse(arg1, arg2)
        // return a value
        return 50
    }
    ```
    - You can return multiple items with `return (50, 100)`
    - Returned items can be assigned with `$x $y method()`
  - Movie main
    - Using raw input
      - Using multiple actions in a single frame
        - `mouse(50, 50) | key(W, A, S, D)`
        - You can separate this with a newline like so
            #### mouse(50, 50)
            #### key(W, A, S, D)
      - Proceeding to the next frame
        - `mouse(50, 50); key(W, A, S, D)`
        - This will move the mouse to 50, 50 in the first frame, then press the keys on the next
        - Multiple lines would look like this
            #### mouse(50, 50);
            #### key(W, A, S, D)
      - Repeating actions
        - `loop 5 { key(W); }`
          - This holds the W key for 5 frames
        - `loop 5 { key(W) | mouse(50, 50); }`
          - This holds the W key and hold the mouse position for 5 frames
        - Can be spread across multiple lines, only need to keep the `{}` brackets balanced
      - Usable input methods
        - `mouse(X_AXIS, (optional Y_AXIS))`
        - `key(KEYCODE, KEYCODE, KEYCODE...)`
    - Built in stuff
      - Loop
        - `loop COUNT { // actions to loop }`
        - Needs matching `{}` brackets
      - Variables
        - Uses key `$` for defining / using
        - Defined like `$name_without_space=50`
        - Used like `Mouse($name_without_space, $name_without_space)`
        - Methods can access variables on main scope
        - Main scope can't access variables inside scopes
        - Loops can access variables on main scope or method scope its used on
      - Methods
        - Can be called with `method(arg1, arg2, ...)`
        - Return values can be assigned to a variable `$method_return method(arg1, arg2)`
      - Action merge / separator
        - Those are the same:

            #### mouse(50, 50)
            #### key(W)
            ---
            #### mouse(50, 50) |
            #### key(W)
            ---
            #### mouse(50, 50) | key(W)
        - You need to use `;` in order to "advance" to the next frame
            #### mouse(50, 50); key(W)
        - You can do multiple `;` chains to advance multiple frames
      - Wait
        - `wait(50)` procudes 50 of `;`
      - Game FPS
        - `fps(100)` will set game FPS to 100
        - `frametime(0.01)` will set the FPS to 100, not all unity versions support frametime so rounds down making the FPS an integer
        - `get_fps() / get_frametime()` returns current fps / frametime
        - If vsync is enabled, it will prevent the FPS from being changed unless vsync is off
      - Registering a loop
        - `register_loop(method_name, initial_arg1, initial_arg2, ...)`
        - `loop_arg(loop_uid, arg1, arg2, ...)`
        - `remove_loop(loop_uid)`
        - Runs the method in a loop along side the main script
        - Example
        ```
        fn bunny_hop() {
          // presses space when on ground
        }
        fn spam_key(key_code) { key(key_code); }

        // registers methods to run 
        $bhop_register register_loop(bunny_hop)
        $spam_register register_loop(spam_key, A)
        $spam_register2 register_loop(spam_key, B)

        // some inputs...

        // change the argument to some registered loop
        loop_arg($spam_register, C)

        // some inputs...

        // stop a loop
        remove_loop($spam_register)
        ```
      - Save state
        - `save(file_path)` to save
```
// example movie
version 1.0.0
name Test TAS
description Does stuff
author yuu0141
startup
os windows
seed 0
// separates properties from main body
---
// some actions here
```

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