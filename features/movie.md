# Movie structure
- Comments: `// comment`
- Movie properties
  - Movie format version - version
    - Required to be at the start of the movie
    - Semantic
  - Virtual environment
      - From startup - from_startup
      - Operating system - os
      - Date time / seed to start with - datetime / seed
      - Frametime or FPS - frametime ft / fps
      - Window resolution - resolution x y
      - Window focus - unfocused
      - From save state (state flag with path to save state) - from_savestate
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
    - Can copy variables with `$variable_copy $variable_source`
    - Const
    - Anything hardcoded in the script like $value=5
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
    - Window resolution
    - `resolution(1920, 1080)`
    - Window focus
    - `window_focus(true) window_focus(false)`
    - Window full screen
    - `window_fullscreen(true) window_fullscreen(false)`
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
        - Accessed through `$value list_index($list, INDEX)`
        - Set through `list_set($list, INDEX, VALUE)`
        - Option type
        - Wraps another type
        - State will be either Some or None
        - Used for some methods that can return "nothing" for example, a helper method that gets a gameobject based on name, but can't find the object
        - Check if Some or None with `is_some($value)` `is_none($value)`
        - Get value of some with `$value get_some($value)`
        - New some instance with `$optional some($value)`
        - New none instance with `$optional none()`
    - Tuple:
        - Can return multiple variables with different types with `return ($value, $value_with_different_time)`
        - Assign to multiple variables with `$value $value2 some_method_with_tuple_return()`
        - Doing `$value some_method_with_tuple_return()` for a return of multiple tuples will only retrieve the first tuple
    - Errors
    - No error handling
    - Any errors like method errors will stop the execution of the movie / not parse and inform the user on error
    - Scopes
    - `{}`
    - Variables inside scopes can't be accessed from the outside
    - Variables outside can be accessed from within the scopes
    - Variables on the main scope can be accessed from any scope
    - Low level stuff
    - Registers
        - Holds a value
        - Tuples can be extended / created with PushTuple REGISTER_DEST REGISTER_SOURCE
        - List can be extended / created with PushList REGISTER_DEST REGISTER_SOURCE
        - Types
        - temp
        - temp2
        - temp3
        - temp4
        - temp5
        - ret
            - Return value
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
        - NewVariable NAME REGISTER
        - SetVariable NAME REGISTER
    - Loops
        - Structured like so
        ```
        // loop initialize
        ConstToRegister temp 10 // loop count
        PushStack temp
        EnterScope
        
        // inside stuff

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
    - TODO Solve issue of running out of registers by creating a nested list
        - Solution involves stacks but i cba to think rn
# Tokens
- $
  - Variable
    - Name
- =, +=, -=, *=, /=, %=
  - Assignment
    - Type of assignment
- -, +, *, /, %
  - MathOp
    - Type of operation
- ()
  - RoundBracket
    - Open or closed
- {}
  - CurlyBracket
    - Open or closed
- []
  - SquareBracket
    - Open or closed
- ,
  - Comma
- Integers
  - Int
- Float
  - Float
- "
  - DoubleQuotes
- \char
  - EscapeChar
    - Char
- ;
  - FrameAdvance
- |
  - SeparateAction
- && || == != ! > < >= <=
  - Logic
    - Logic type
- anything else
  - Identity
```
$value = 20
$value2 = $value
$value = 40
$value3 = (50, 10, 20, "something")
fn method(arg1, arg2) {
if true && false {
    keyboard(W) | keyboard(A)
    ;;;
}
if $arg1 {
    echo("test")
} else {
    ;
}

return (arg + arg2, 50, "", [0.0, 1.0, 2.0, 3.1 * 50.0])
}
$value4 = (0, 0, "", [])
loop 10 {
$value4 = method($value, 10)
}
loop 2 {
break
}

// ====================== ASM ============================

// method define
PopArg temp
NewVariable arg2 temp // right to left
PopArg temp
NewVariable arg1 temp

ConstToRegister temp true
ConstToRegister temp2 false
And temp temp temp2
JumpIfTrue temp 10 // jump to after if statement

ConstToRegister temp W
PushArg temp
GotoMethod Keyboard

ConstToRegister temp A
PushArg temp
GotoMethod Keyboard

FrameAdvance
FrameAdvance
FrameAdvance

VarToRegister arg1 temp
JumpIfFalse temp 4 // goes to else statement

ConstToRegister temp "test"
GotoMethod echo
Jump 2 // goes after else statement

FrameAdvance

VarToRegister arg temp
VarToRegister arg2 temp2
Add ret temp temp2
ConstToRegister temp 50
PushTuple ret temp
ConstToRegister temp ""
PushTuple ret temp
ConstToRegister temp 0.0
ConstToRegister temp2 1.0
PushList temp temp2
ConstToRegister temp2 2.0
PushList temp temp2
ConstToRegister temp2 3.1
ConstToRegister temp3 50.0
Mul temp2 temp2 temp3
PushList temp temp2
PushTuple ret temp
Return

// main
ConstToRegister temp 20
NewVariable value temp

VarToRegister temp value
NewVariable value2 temp

ConstToRegister temp 40
SetVariable value temp

ConstToRegister temp (50, 10, 20, "something")
NewVariable value3 temp

ConstToRegister temp (0, 0, "", [])
NewVariable value4 temp

ConstToRegister temp 10
PushStack temp
EnterScope

VarToRegister temp value
PushArg temp
ConstToRegister temp 10
PushArg temp
GotoMethod method
SetVariable value4 ret

PopStack temp
ConstToRegister temp2 1
Sub temp temp temp2
PushStack temp
ConstToRegister temp2 0
JumpIfGt temp temp2 -11
ExitScope
PopStack temp

ConstToRegister temp 2
PushStack temp
EnterScope

Jump 7 // aim towards the matching ExitScope

PopStack temp
ConstToRegister temp2 1
Sub temp temp temp2
PushStack temp
ConstToRegister temp2 0
JumpIfGt temp temp2 -6
ExitScope
PopStack temp
```
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