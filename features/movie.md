# Movie structure
- Comments:
  - `// comment`
  - `/* comment with any amount of lines */`
- Movie properties
  - Virtual environment
      - Operating system - os
      - Date time / seed to start with - datetime / seed
      - Frametime or FPS - frametime ft / fps
      - Window state
        - Window resolution - resolution x y
        - Window focus - unfocused
        - full screen - fullscreen
    - From save state - from_savestate PATH
      - Load environment settings from state
  - Name - name
  - Description - desc
  - Author - author
  - End save - endsave path
      - Creates a save state at end for using it on another movie
- Scripting
    - Loop
      - `loop COUNT { // actions to loop }`
      - Needs matching `{}` brackets
    - Variables
      - Uses key `$` for defining / using
      - Defined like `$name_without_space = 50`
      - Used like `Mouse($name_without_space, $name_without_space)`
      - Methods can access variables on main scope
      - Main scope can't access variables inside scopes
      - Loops can access variables on main scope or method scope its used on
      - Can copy variables with `$variable_copy = $variable_source`
    - Const
      - Anything hardcoded in the script like $value = 5
    - Methods
      - Can be called with `method(arg1, arg2, ...)`
      - Return values can be assigned to a variable `$method_return = method(arg1, arg2)`
    - Action separator
      - Those are the same:
        #### mouse(50, 50)
        #### key(W)
        ---
        #### mouse(50, 50) |
        #### key(W)
        ---
        #### mouse(50, 50) | key(W)

      - You can separate actions with `;` as well
      - Variable assignment, method calls is separatable
    - You need to use `;` in order to "advance" to the next frame
        #### mouse(50, 50); key(W)
    - You can do multiple `;` chains to advance multiple frames
    - If
      - if $condition { // stuff }
      - if $condition { } else { }
      - if $condition { } else if $condition { } else...
      - Method that returns a boolean can directly be used on the condition without assigning to variable
    - Variable types
      - Once assigned, type of the variable can't be changed
      - Types:
        - Int
        - Float
        - String
        - Bool
        - List of types
          - New instance from `$value = list_new(VALUE, VALUE2, ...)`
          - Accessed through `$value = list_index($list, INDEX)`
          - Set through `list_set($value, INDEX, VALUE)`
          - Added through `list_add($value, VALUE)`
          - Remove item `list_remove($value, INDEX)`
        - Option type
          - Wraps another type
          - State will be either Some or None
          - Used for some methods that can return "nothing" for example, a helper method that gets a gameobject based on name, but can't find the object
          - Check if Some or None with `is_some($value)` `is_none($value)`
          - Get value of some with `$value = get_some($value)`
          - New some instance with `$optional = some($value)`
          - New none instance with `$optional = none()`
        - Custom type script implementation
          - Option
            - "None<>" "None<TYPE>" "Some<TYPE>(VALUE)"
          - List
            - "List<TYPE>(VALUE, VALUE, ...)" "List<List<TYPE>>((VALUE, VALUE), (VALUE))"
    - Tuple:
        - Can return multiple variables with different types with `return ($value, $value_with_different_time)`
        - Assign to multiple variables with `($value, $value2) = tuple_method() some_method_with_tuple_return()`
        - This works too `($value, $value2) = $tuple_value`
        - You can skip tuple value with _ `(_, $value2) = $tuple_value`
        - Doing `($value) = some_method_with_tuple_return()` for a return of multiple tuples will only retrieve the first tuple
        - Doing `$tuple_copy = $tuple` will copy the tuple
    - Errors
      - No error handling
      - Any errors like method errors will stop the execution of the movie / not parse and inform the user on error
    - Scopes
      - `{}`
      - Variables inside scopes can't be accessed from the outside
      - Variables outside can be accessed from within the scopes
      - Variables on the main scope can be accessed from any scope
    - Return
      - Returns from method to outer
      - Can add expression like `return EXPRESSION`
      - Using this in main will end the movie execution

# Low level stuff
- Registers
    - Holds a value
    - Tuples can be extended / created with PushTuple REGISTER_DEST REGISTER_SOURCE
    - Tuples can be popped with PopTuple REGISTER_DEST REGISTER_SOURCE
    - List can be extended / created with PushList REGISTER_DEST REGISTER_SOURCE
    - Types
      - temp
      - temp2
      - temp3
      - temp4
      - temp5
      - ret
- Stack
    - PushStack temp
    - Pushes temp register by copying
    - PopStack temp
    - Pops temp register after clearing it
- Method defining
    - Defined method will be store in a different "section" of the engine, which can't be reached with Jump
- Method call
    - GotoMethod METHOD_NAME
    - Engine will store stack of return indexes and what section
    - Engine will automatically know it has entered a scope and when it exits it
    - Registers won't be cleared or altered in any way by the engine through method jumps
    - Arguments
    - PushArg temp
        - Pushes register to the argument stack
    - PopArg temp
        - Pops argument stack to register
    - Returning to call origin
    - Return
- Jump
    - Jump Offset
- Comparison
    - Those compare all the register index values with the other
    - JumpIfEq REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER == REGISTER2
    - JumpIfNEq REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER != REGISTER2
    - JumpIfLT REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER < REGISTER2
    - JumpIfGT REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER > REGISTER2
    - JumpIfLTEq REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER <= REGISTER2
    - JumpIfGTEq REGISTER REGISTER2 JUMP_OFFSET
    - REGISTER >= REGISTER2
    - JumpIfTrue REGISTER JUMP_OFFSET
    - JumpIfFalse REGISTER JUMP_OFFSET
- Maths
    - Add RESULT_REGISTER REGISTER REGISTER2
    - Sub RESULT_REGISTER REGISTER REGISTER2
    - Mult RESULT_REGISTER REGISTER REGISTER2
    - Div RESULT_REGISTER REGISTER REGISTER2
    - Mod RESULT_REGISTER REGISTER REGISTER2
- Logic
    - And RESULT_REGISTER REGISTER REGISTER2
    - Or RESULT_REGISTER REGISTER REGISTER2
    - Not RESULT_REGISTER REGISTER REGISTER2
    - Xor RESULT_REGISTER REGISTER REGISTER2
- Scopes
    - EnterScope
    - ExitScope
- Variables
  - SetVariable NAME REGISTER
- Loops
    - Structured like so
    ```
    // loop initialize
    ConstToRegister temp 10 // loop count
    PushStack temp
    EnterScope
    
    // inside opcode

    PopStack temp
    ConstToRegister temp2 1
    Sub temp temp temp2
    PushStack temp
    ConstToRegister temp2 0
    JumpIfGt temp temp2 -6 // jump to start of the inside stuff
    ExitScope // if break is called, will jump to this line
    PopStack temp
    ```
- Advance movie by a frame
  - FrameAdvance
- Register operation
  - Move REGISTER REGISTER2
    - Moves REGISTER2 to REGISTER
- Expression operation brackets problem
  - Using brackets in expressions temporarily takes up a register, so if you have a lot of (expression), you will run out of registers
  - To solve this, use a temp register to store the result of the expression, and use the stack to store the temp register if you need to use it again

# Helper methods
- Save state
  - `save(file_path)` to save
- Window resolution
  - `resolution(1920, 1080)`
- Window focus
  - `window_focus(true) window_focus(false)`
- Window full screen
  - `window_fullscreen(true) window_fullscreen(false)`
- Registering a loop
  - `register_loop(method_name, initial_arg1, initial_arg2, ...)`
  - `loop_arg(loop_uid, arg1, arg2, ...)`
  - `remove_loop(loop_uid)`
  - Runs the method in a loop along side the main script and other loops
  - Each time the method "exits", the method will be called again on the next frame
  - Example
  ```
  fn bunny_hop() {
      // presses space when on ground
  }
  fn spam_key(key_code) { key(key_code); }

  // registers methods to run 
  $bhop_register = register_loop(bunny_hop)
  $spam_register = register_loop(spam_key, A)
  $spam_register2 = register_loop(spam_key, B)

  // some inputs...

  // change the argument to some registered loop
  loop_arg($spam_register, C)

  // some inputs...

  // stop a loop
  remove_loop($spam_register)
  ```
- Wait
  - `wait(50)` procudes 50 of `;`
- Game FPS
  - `fps(100)` will set game FPS to 100
  - `frametime(0.01)` will set the FPS to 100, not all unity versions support frametime so rounds down making the FPS an integer
  - `get_fps() / get_frametime()` returns current fps / frametime
  - If vsync is enabled, it will prevent the FPS from being changed unless vsync is off

# Reserved words
- `fn` for function defining
- `if` for if statement
- `else` for else statement
- `loop` for looping statement
- `for` RESERVED FOR FUTURE USE
- `while` RESERVED FOR FUTURE USE
- `break` for breaking out of loops
- `continue` for continuing to the next iteration of a loop
- `return` for returning from a method
- `true` for true
- `false` for false

# BNF description
Refer to [antlr grammer](../Plugin/antlr/MovieScriptDefaultGrammar.g4)

# Example
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
fn method() {
  // advance 5 frames
  ;;;;;
  return 50
}

$var = method()
print($var)

press_left();;;;; unpress_left()
press_right() | press_forward() ;;;;;;;;
```