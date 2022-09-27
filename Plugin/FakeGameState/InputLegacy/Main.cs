namespace UniTASPlugin.FakeGameState.InputLegacy;

public static class Main
{
    public static void Update()
    {
        if (UniTASPlugin.TAS.Running)
        {
            Mouse.Update();
            Keyboard.Update();
            //VirtualCursor.Update();
        }
    }

    public static void Clear()
    {
        Mouse.Clear();
        Keyboard.Clear();
        Axis.Clear();
    }
}