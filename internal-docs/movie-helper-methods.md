# Game fps
`set_fps(fps: float)`
- Sets new fps, has a delay of 1 frame

`set_frametime(frametime: float)`
- Sets new fps using frametime, has a delay of 1 frame

`get_fps() -> float`
- Gets current fps, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet

`get_frametime() -> float`
- Gets current frametime, if you changed the fps with `set_fps` or `set_frametime` then this will return that framerate even if not applied yet