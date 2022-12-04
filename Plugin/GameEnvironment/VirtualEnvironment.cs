using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.Input;

namespace UniTASPlugin.GameEnvironment;

public class VirtualEnvironment
{
    public VirtualEnvironment()
    {
        RunVirtualEnvironment = false;
        Os = Os.Windows;
        WindowState = new WindowState(1920, 1080, false, true);
        InputState = new InputState();
    }

    public bool RunVirtualEnvironment { get; set; }
    public Os Os { get; }
    public WindowState WindowState { get; }
    public InputState InputState { get; }
}