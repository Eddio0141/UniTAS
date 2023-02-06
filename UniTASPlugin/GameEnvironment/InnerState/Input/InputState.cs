namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class InputState
{
    public MouseState MouseState { get; } = new();
    public AxisState AxisState { get; } = new();
    public KeyboardState KeyboardState { get; } = new();
    public ButtonState ButtonState { get; } = new();

    public void ResetStates()
    {
        MouseState.ResetState();
        AxisState.ResetState();
        KeyboardState.ResetState();
        ButtonState.ResetState();
    }

    public void Update()
    {
        MouseState.Update();
        AxisState.Update();
        KeyboardState.Update();
        ButtonState.Update();
    }
}