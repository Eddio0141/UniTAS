using System;

namespace UniTASPlugin.Movie;

public class Framebulk
{
    public float Frametime;
    public int FrameCount;

    public Mouse Mouse;
    public Keys Keys;
    public Axises Axises;

    public Framebulk() : this(0.01f, 1, new Mouse(), new Keys(), new Axises()) { }
    public Framebulk(float frametime, uint frameCount) : this(frametime, frameCount, new Mouse(), new Keys(), new Axises()) { }
    public Framebulk(float frametime, uint frameCount, Mouse mouse) : this(frametime, frameCount, mouse, new Keys(), new Axises()) { }
    public Framebulk(float frametime, uint frameCount, Keys key) : this(frametime, frameCount, new Mouse(), key, new Axises()) { }
    public Framebulk(float frametime, uint frameCount, Axises axis) : this(frametime, frameCount, new Mouse(), new Keys(), axis) { }
    public Framebulk(float frametime, uint frameCount, Mouse mouse, Keys key, Axises axis)
    {
        Frametime = frametime;
        FrameCount = (int)frameCount;
        Mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        Keys = key ?? throw new ArgumentNullException(nameof(key));
        Axises = axis ?? throw new ArgumentNullException(nameof(axis));
    }
}
