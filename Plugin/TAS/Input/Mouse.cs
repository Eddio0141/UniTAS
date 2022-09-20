using UnityEngine;

namespace UniTASPlugin.TAS.Input;

public static class Mouse
{
    public static bool MousePresent { get; internal set; } = true;
    public static Vector2 Position { get; internal set; } = Vector2.zero;
    public static bool LeftClick { get; internal set; } = false;
    public static bool LeftClickDown { get; private set; } = false;
    public static bool LeftClickUp { get; private set; } = false;
    private static bool LeftClickPrev = false;
    public static bool RightClick { get; internal set; } = false;
    public static bool RightClickDown { get; private set; } = false;
    public static bool RightClickUp { get; private set; } = false;
    private static bool RightClickPrev = false;
    public static bool MiddleClick { get; internal set; } = false;
    public static bool MiddleClickDown { get; private set; } = false;
    public static bool MiddleClickUp { get; private set; } = false;
    private static bool MiddleClickPrev = false;

    public static void Update()
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

    public static void Clear()
    {
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
}