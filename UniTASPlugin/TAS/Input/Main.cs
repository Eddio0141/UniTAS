using UniTASPlugin.TAS.Input.Movie;

namespace UniTASPlugin.TAS.Input;

public static class Main
{
    public static void Update()
    {
        if (TAS.Main.Running)
        {
            // TODO button handler
            Mouse.Update();
            Keyboard.Update();
            VirtualCursor.Update();
        }
    }

    public static void Clear()
    {
        Mouse.Clear();
        Keyboard.Clear();
        Axis.Clear();
    }
}