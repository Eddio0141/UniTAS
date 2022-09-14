using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniTASPlugin.TAS.Input.Movie;

public static class MovieHandler
{
    public static Movie CurrentMovie { get; private set; }
    public static ulong CurrentFrameNum { get; private set; }
    static int currentFramebulkIndex;
    static int currentFramebulkFrameIndex;

    public static void RunMovie(Movie movie)
    {
        CurrentFrameNum = 0;
        currentFramebulkFrameIndex = 0;
        currentFramebulkIndex = 0;

        CurrentMovie = movie;

        TAS.Main.Running = true;
        TAS.Main.SoftRestart(CurrentMovie.Seed);

        Plugin.Log.LogInfo($"Running movie {CurrentMovie.Name} with {CurrentMovie.TotalFrames()} frames ({CurrentMovie.TotalSeconds()}s runtime)");
    }

    static bool CheckCurrentMovieEnd()
    {
        if (currentFramebulkIndex >= CurrentMovie.Framebulks.Count)
        {
            TAS.Main.Running = false;
            return false;
        }

        return true;
    }

    public static void Update()
    {
        if (TAS.Main.Running)
        {
            if (!CheckCurrentMovieEnd())
                return;

            var fb = CurrentMovie.Framebulks[currentFramebulkIndex];
            if (currentFramebulkFrameIndex >= fb.FrameCount)
            {
                currentFramebulkIndex++;
                if (!CheckCurrentMovieEnd())
                    return;

                currentFramebulkFrameIndex = 0;
                fb = CurrentMovie.Framebulks[currentFramebulkIndex];
            }

            Input.Mouse.Position = new Vector2(fb.Mouse.X, fb.Mouse.Y);
            Input.Mouse.LeftClick = fb.Mouse.Left;
            Input.Mouse.RightClick = fb.Mouse.Right;
            Input.Mouse.MiddleClick = fb.Mouse.Middle;

            var axisMoveSetDefault = new List<string>();
            foreach (var (key, _) in Input.Axis.Values)
            {
                if (!fb.Axis.AxisMove.ContainsKey(key))
                    axisMoveSetDefault.Add(key);
            }
            foreach (var key in axisMoveSetDefault)
            {
                if (Input.Axis.Values.ContainsKey(key))
                    Input.Axis.Values[key] = default;
                else
                    Input.Axis.Values.Add(key, default);
            }
            foreach (var (axis, value) in fb.Axis.AxisMove)
            {
                if (Input.Axis.Values.ContainsKey(axis))
                {
                    Input.Axis.Values[axis] = value;
                }
                else
                {
                    Input.Axis.Values.Add(axis, value);
                }
            }

            CurrentFrameNum++;
            currentFramebulkFrameIndex++;
        }
    }
}

public class Movie
{
    public readonly string Name;
    public readonly List<Framebulk> Framebulks;
    public readonly int Seed;

    public Movie(string name, List<Framebulk> framebulks) : this(name, framebulks, 0) { }

    public Movie(string name, List<Framebulk> framebulks, int seed)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Framebulks = framebulks ?? throw new ArgumentNullException(nameof(framebulks));
        Seed = seed;
    }

    public float TotalSeconds()
    {
        return Framebulks.Sum(f => f.FrameCount * f.Frametime);
    }

    public float TotalFrames()
    {
        return Framebulks.Sum(f => f.FrameCount);
    }
}

public class Framebulk
{
    public float Frametime;
    public int FrameCount;

    public Mouse Mouse;
    public Key Key;
    public Axis Axis;

    public Framebulk(float frametime, int frameCount) : this(frametime, frameCount, new Mouse(), new Key(), new Axis()) { }

    public Framebulk(float frametime, int frameCount, Mouse mouse, Key key, Axis axis)
    {
        Frametime = frametime;
        FrameCount = frameCount;
        Mouse = mouse ?? throw new ArgumentNullException(nameof(mouse));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        Axis = axis ?? throw new ArgumentNullException(nameof(axis));
    }
}

public class Mouse
{
    public float X;
    public float Y;

    public bool Left;
    public bool Right;
    public bool Middle;
    // TODO scroll

    public Mouse() : this(0, 0, false, false, false) { }

    public Mouse(float x, float y) : this(x, y, false, false, false) { }

    public Mouse(float x, float y, bool left) : this(x, y, left, false, false) { }

    public Mouse(float x, float y, bool left, bool right) : this(x, y, left, right, false) { }

    public Mouse(float x, float y, bool left, bool right, bool middle)
    {
        X = x;
        Y = y;
        Left = left;
        Right = right;
        Middle = middle;
    }
}

public class Key
{
    public List<KeyCode> Pressed;

    public Key() : this(new()) { }

    public Key(List<KeyCode> pressed)
    {
        Pressed = pressed ?? throw new ArgumentNullException(nameof(pressed));
    }
}

public class Axis
{
    public Dictionary<string, float> AxisMove;

    public Axis() : this(new()) { }

    public Axis(Dictionary<string, float> axisMove)
    {
        AxisMove = axisMove;
    }
}