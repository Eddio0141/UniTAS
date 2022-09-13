using UnityEngine;

namespace UniTASPlugin.TAS;

public static class Input
{
    public static Mouse Mouse;
    static int testState = 0;

    static Input()
    {
        Mouse = new Mouse();
    }

    public static void Update()
    {
        // TODO remove this test
        if (TASTool.Time < 2)
        {
            if (testState == 0)
            {
                Plugin.Log.LogDebug($"test state 0 at time {TASTool.Time}");
                testState++;
            }

            Mouse.Position = new Vector2(300, 700);
        }
        else if (TASTool.Time < 3)
        {
            if (testState == 1)
            {
                Plugin.Log.LogDebug($"test state 1 at time {TASTool.Time}");
                testState++;
            }

            Mouse.LeftClick = true;
        }
        else if (TASTool.Time < 3.1)
        {
            if (testState == 2)
            {
                Plugin.Log.LogDebug($"test state 2 at time {TASTool.Time}");
                testState++;
            }

            Mouse.LeftClick = false;
        }
        else if (TASTool.Time > 6 && TASTool.Running)
        {
            if (testState == 3)
            {
                Plugin.Log.LogDebug($"test state 3 at time {TASTool.Time}");
                testState++;
            }

            Plugin.Log.LogDebug("finished");
            TASTool.Running = false;
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
