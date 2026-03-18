Document on how UniTAS works

# Unity engine internal

## Finding unity c++ to c# function binding

### 2017.4.6 Linux

This loop calls functions that binds Unity C# extern functions to Unity C++

```asm
LEA        RBP,[LinkFunctionsStop]
PUSH       RBX
LEA        func,[LinkFunctionsBase]
NOP        word ptr [RAX + RAX*0x1]
LOOP:
CALL       qword ptr [func]
ADD        func,0x8
CMP        func,RBP
JNZ        LOOP
```

Some of these functions also binds multiple functions through a loop, which loops like this

```c
name = bind_names;
i = 0;
  do {
    bind(name, bind_funcs[i]);
    name = bind_names[i];
    i += 1;
  } while (name != nullptr);
```

While some just does this

```c
void screen_bindings() {
  bind("UnityEngine.Screen::get_width",get_width);
  bind("UnityEngine.Screen::get_height",get_height);
}
```

Knowing this, this is how you would find the C++ function for a unity function bind

1. Find unity function name string
    - Example: `UnityEngine.Object::set_name`
2. Find references to string
    - If referenced in `.text`, that should be where it is used in the binding function, so function is referenced along this string
    - Continue to step 3 if referenced in `.rodata`
3. From the address in step 2, find a 16 byte `00` padding backwards, and take the address right after the padding
4. Search for reference to step 3, it should lead to the function that binds multiple functions in a loop
    - The base function would be referenced in this function
    - You can figure out which function is linked to the name by adding the offset of the string array from step 3 to the base function address

# Game restart

## Unity engine internals

Ideally internal state is reset over spoofing it on c# side

### Scene state

TODO: implementation

There is a function to initialize `RuntimeSceneManager` and assigns it to a global variable, and another function to cleanup

Those functions are registered as init and cleanup via the `RegisterRuntimeInitializeAndCleanup` class, where those functions are passed as constructor arguments

```asm
LEA RSI,[InitRuntimeSceneManager]
LEA RDX,[CleanupRuntimeSceneManager]
```

Search for init / cleanup is done with the following steps at runtime
1. Searching for the `RuntimeSceneManager` class constructor
2. `init` function's assembly code with `CALL` using the previous search result
3. Search for `init` function usage with the `LEA` instruction

This should land in an exported function `_INIT_<integer number>`
