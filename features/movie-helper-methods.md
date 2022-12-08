# Concurrent runner
`register(methodName: string, preUpdate: bool, defaultArg1, defaultArg2, ...) -> int`
- Registers a method into the concurrent runner, looping forever
- Able to register any number of methods, even ones with the same name
- Returns a hashcode, used later for removing registered concurrent runner

`unregister(hashcode: int, preUpdate: bool)`
- Removes registered method using registered index

# Input control
## Keyboard
`hold_key(key: string)`
- Holds keyboard key

`unhold_key(key: string)`
- Removes held key

`clear_held_keys()`
- Removes all held key

## Mouse
`move_mouse(x: int, y: int)`
- Moves mouse to there

## Controller

# Game fps
`set_fps(fps: float)`
- Sets new fps

`set_frametime(frametime: float)`
- Sets new fps using frametime

`get_fps() -> float`
- Gets current fps

`get_frametime() -> float`
- Gets current frametime