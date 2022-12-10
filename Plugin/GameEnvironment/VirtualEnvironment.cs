using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.Input;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// A class holding current virtual environment of the system the game is running on
/// </summary>
public class VirtualEnvironment
{
    private bool _runVirtualEnvironment;

    public bool RunVirtualEnvironment
    {
        get => _runVirtualEnvironment;
        set
        {
            if (_runVirtualEnvironment)
            {
                InputState.ResetStates();
            }

            _runVirtualEnvironment = value;
        }
    }

    public Os Os { get; } = Os.Windows;
    public WindowState WindowState { get; } = new(1920, 1080, false, true);
    public InputState InputState { get; } = new();
    public float FrameTime { get; set; }
    public bool Restart { get; set; }
}