using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.Input;

namespace UniTASPlugin.GameEnvironment;

public class VirtualEnvironment
{
    public bool RunVirtualEnvironment { get; set; }
    public Os Os { get; } = Os.Windows;
    public WindowState WindowState { get; } = new(1920, 1080, false, true);
    public InputState InputState { get; } = new();
    public float FrameTime { get; set; }
    public bool Restart { get; set; }
}