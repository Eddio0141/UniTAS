namespace UniTASPlugin.GameEnvironment;

public class GameEnvironment
{
    public static GameEnvironment Instance { get; } = new GameEnvironment();

    private GameEnvironment()
    {
        // TODO copy actual environment settings
        Os = Os.Windows;
        WindowState = new WindowState(1920, 1080, false, true);
    }

    public Os Os { get; }
    public WindowState WindowState { get; }
}