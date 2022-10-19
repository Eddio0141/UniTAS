namespace UniTASPlugin.GameEnvironment;

public class GameEnvironment
{
    public static GameEnvironment Instance { get; } = new GameEnvironment();

    private GameEnvironment()
    {
        // TODO copy actual environment settings
        OverrideEnvironment = false;
        Os = Os.Windows;
        WindowState = new WindowState(1920, 1080, false, true);
        InputState = new InputState();
    }

    public bool OverrideEnvironment { get; set; }

    public Os Os { get; }
    public WindowState WindowState { get; }
    public InputState InputState { get; }
}