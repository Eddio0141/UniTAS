using UniTASPlugin.TAS.Input.Movie;

namespace UniTASPlugin.TAS.Input;

public static class Main
{
    public static void Update()
    {
        // TODO button handler
        MovieHandler.Update();
        Mouse.Update();
        Keyboard.Update();
        VirtualCursor.Update();
    }

    public static void Clear()
    {
        Mouse.Clear();
        Keyboard.Clear();
        Axis.Clear();
    }
}