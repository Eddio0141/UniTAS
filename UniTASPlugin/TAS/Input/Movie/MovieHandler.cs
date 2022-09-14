using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.TAS.Input.Movie;

public static class MovieHandler
{
    public static Movie CurrentMovie { get; private set; }
    public static int CurrentFrameNum { get; private set; }

    public static void RunMovie(Movie movie)
    {
        CurrentMovie = movie;
        TAS.Main.Running = true;
        TAS.Main.SoftRestart();
    }

    public static void Update()
    {
        if (TAS.Main.Running)
        {
            if (CurrentFrameNum >= CurrentMovie.Framebulks.Count)
            {
                TAS.Main.Running = false;
                return;
            }

            var fb = CurrentMovie.Framebulks[CurrentFrameNum];

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
        }
    }
}

public class Movie
{
    public List<Framebulk> Framebulks;
}

public class Framebulk
{
    public float Frametime;
    public ulong FrameCount;

    public Mouse Mouse;
    public Key Key;
    public Axis Axis;
}

public class Mouse
{
    public float X;
    public float Y;

    public bool Left;
    public bool Right;
    public bool Middle;

    // TODO scroll
}

public class Key
{
    public List<KeyCode> Pressed;
}

public class Axis
{
    public Dictionary<string, float> AxisMove;
}