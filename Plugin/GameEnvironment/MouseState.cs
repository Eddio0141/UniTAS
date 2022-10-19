using UnityEngine;

namespace UniTASPlugin.GameEnvironment;

public class MouseState
{
    public bool MousePresent { get; internal set; } = true;
    public Vector2 Position { get; internal set; } = Vector2.zero;
    public bool LeftClick { get; internal set; } = false;
    public bool LeftClickDown { get; private set; } = false;
    public bool LeftClickUp { get; private set; } = false;
    private bool LeftClickPrev = false;
    public bool RightClick { get; internal set; } = false;
    public bool RightClickDown { get; private set; } = false;
    public bool RightClickUp { get; private set; } = false;
    private bool RightClickPrev = false;
    public bool MiddleClick { get; internal set; } = false;
    public bool MiddleClickDown { get; private set; } = false;
    public bool MiddleClickUp { get; private set; } = false;
    private bool MiddleClickPrev = false;

    public void Update()
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

    public void Clear()
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