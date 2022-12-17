namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class MouseState : InputDeviceBase
{
    public bool MousePresent { get; set; }
    public float XPos { get; set; }
    public float YPos { get; set; }
    public bool LeftClick { get; set; }
    public bool LeftClickDown { get; private set; }
    public bool LeftClickUp { get; private set; }
    private bool _leftClickPrev;
    public bool RightClick { get; set; }
    public bool RightClickDown { get; private set; }
    public bool RightClickUp { get; private set; }
    private bool _rightClickPrev;
    public bool MiddleClick { get; set; }
    public bool MiddleClickDown { get; private set; }
    public bool MiddleClickUp { get; private set; }
    private bool _middleClickPrev;

    public MouseState()
    {
        MousePresent = true;
        ResetState();
    }

    public override void Update()
    {
        LeftClickUp = _leftClickPrev && !LeftClick;
        LeftClickDown = !_leftClickPrev && LeftClick;

        RightClickUp = _rightClickPrev && !RightClick;
        RightClickDown = !_rightClickPrev && RightClick;

        MiddleClickUp = _middleClickPrev && !MiddleClick;
        MiddleClickDown = !_middleClickPrev && MiddleClick;

        _leftClickPrev = LeftClick;
        _rightClickPrev = RightClick;
        _middleClickPrev = MiddleClick;
    }

    public sealed override void ResetState()
    {
        XPos = 0f;
        YPos = 0f;

        LeftClick = false;
        LeftClickDown = false;
        LeftClickUp = false;
        _leftClickPrev = false;

        RightClick = false;
        RightClickDown = false;
        RightClickUp = false;
        _rightClickPrev = false;

        MiddleClick = false;
        MiddleClickDown = false;
        MiddleClickUp = false;
        _middleClickPrev = false;
    }
}