using UnityEngine;

namespace UniTASPlugin.FakeGameState.InputLegacy;

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
        LeftClickUp = LeftClickPrev && !LeftClick;
        LeftClickDown = !LeftClickPrev && LeftClick;

        RightClickUp = RightClickPrev && !RightClick;
        RightClickDown = !RightClickPrev && RightClick;

        MiddleClickUp = MiddleClickPrev && !MiddleClick;
        MiddleClickDown = !MiddleClickPrev && MiddleClick;

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