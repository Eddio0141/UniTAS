# Concurrent runner
`register(methodName: string, preUpdate: bool, defaultArg1, defaultArg2, ...) -> int`
- Registers a method into the concurrent runner, looping forever
- Able to register any number of methods, even ones with the same name
- Returns a hash, used later for removing registered concurrent runner

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

`left_click(clicked: bool)`
- Click / unclick mouse left

`right_click(clicked: bool)`
- Click / unclick mouse right

`middle_click(clicked: bool)`
- Click / unclick mouse middle

## Controller
`move_axis(name: string, value: float)`
- Moves axis `name` to value `value`

# Game fps
`set_fps(fps: float)`
- Sets new fps, has a delay of 1 frame

`set_frametime(frametime: float)`
- Sets new fps using frametime, has a delay of 1 frame

`get_fps() -> float`
- Gets current fps, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet

`get_frametime() -> float`
- Gets current frametime, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet