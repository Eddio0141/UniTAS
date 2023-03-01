# Input control
## Keyboard
`hold_key(key: string)`
- Holds keyboard key

`release_key(key: string)`
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

`hold_button(button_name: string)`
- Holds button `button_name`

`release_button(button_name: string)`
- Releases button `button_name`

`clear_held_buttons()`
- Releases all held buttons

# Game fps
`set_fps(fps: float)`
- Sets new fps, has a delay of 1 frame

`set_frametime(frametime: float)`
- Sets new fps using frametime, has a delay of 1 frame

`get_fps() -> float`
- Gets current fps, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet

`get_frametime() -> float`
- Gets current frametime, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet