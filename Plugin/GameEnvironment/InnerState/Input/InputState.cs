namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public class InputState
{
    public MouseState MouseState { get; } = new();
    public AxisState AxisState { get; } = new();
    public KeyboardState KeyboardState { get; } = new();

    public void ResetStates()
    {
        MouseState.ResetState();
        AxisState.ResetState();
        KeyboardState.ResetState();
    }
}