using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.FakeGameState.InputLegacy;

public static class Main
{
    public static void Update()
    {
        if (UniTASPlugin.TAS.Running)
        {
            MouseState.Update();
            Keyboard.Update();
            //VirtualCursor.Update();
        }
    }

    public static void Clear()
    {
        MouseState.Clear();
        Keyboard.Clear();
        Axis.Clear();
    }
}