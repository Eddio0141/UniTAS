using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.TAS;

public static class Input
{
    public static Mouse Mouse;
    public static Dictionary<string, float> Axis { get; internal set; }

    static Input()
    {
        Mouse = new Mouse();
        Axis = new Dictionary<string, float>();
    }

    public static void Update()
    {
        // TODO remove this test
        if (Axis.Count == 0)
        {
            Axis.Add("Mouse X", 0f);
            Axis.Add("Mouse Y", 0f);
        }
        if (Main.Time < 2)
        {
            Mouse.Position = new Vector2(300, 700);
        }
        else if (Main.Time < 3)
        {
            Mouse.LeftClick = true;
        }
        else if (Main.Time < 3.1)
        {
            Mouse.LeftClick = false;
        }
        else if (Main.Time < 5)
        {
            Axis["Mouse X"] = 1f;
            Axis["Mouse Y"] = 0.2f;
        }
        else if (Main.Time < 6)
        {
            Axis["Mouse X"] = -2f;
            Axis["Mouse Y"] = 0f;
        }
        else if (Main.Time > 8 && Main.Running)
        {
            Plugin.Log.LogDebug("finished");
            Main.Running = false;
        }

        Mouse.Update();
    }
}

public class Mouse
{
    public bool MousePresent { get; internal set; }
    public Vector2 Position { get; internal set; }
    public bool LeftClick { get; internal set; }
    public bool LeftClickDown { get; private set; }
    public bool LeftClickUp { get; private set; }
    private bool LeftClickPrev;
    public bool RightClick { get; internal set; }
    public bool RightClickDown { get; private set; }
    public bool RightClickUp { get; private set; }
    private bool RightClickPrev;
    public bool MiddleClick { get; internal set; }
    public bool MiddleClickDown { get; private set; }
    public bool MiddleClickUp { get; private set; }
    private bool MiddleClickPrev;

    public Mouse()
    {
        MousePresent = true;
        Position = Vector2.zero;

        LeftClick = false;
        LeftClickDown = false;
        LeftClickUp = false;
        LeftClickPrev = false;

        RightClick = false;
        RightClickDown = false;
        RightClickUp = false;
        RightClickPrev = false;

        MiddleClick = false;
        MiddleClickDown = false;
        MiddleClickUp = false;
        MiddleClickPrev = false;
    }

    public void Update()
    {
        if (LeftClickPrev && !LeftClick)
            LeftClickUp = true;
        else
            LeftClickUp = false;
        if (!LeftClickPrev && LeftClick)
            LeftClickDown = true;
        else
            LeftClickDown = false;

        if (RightClickPrev && !RightClick)
            RightClickUp = true;
        else
            RightClickUp = false;
        if (!RightClickPrev && RightClick)
            RightClickDown = true;
        else
            RightClickDown = false;

        if (MiddleClickPrev && !MiddleClick)
            MiddleClickUp = true;
        else
            MiddleClickUp = false;
        if (!MiddleClickPrev && MiddleClick)
            MiddleClickDown = true;
        else
            MiddleClickDown = false;

        LeftClickPrev = LeftClick;
        RightClickPrev = RightClick;
        MiddleClickPrev = MiddleClick;
    }
}
