using System;

namespace Core.SaveState;

internal class State
{
    // save state info itself
    public readonly UnityVersion SaveVersion;
    // scene
    public readonly int Scene;
    // time stuff
    public readonly DateTime Time;
    public readonly ulong FrameCount;
    public readonly int FixedUpdateIndex;
    // cursor stuff
    public readonly bool CursorVisible;
    public readonly UnityHooks.CursorLockModeType CursorLockState;

    public State(int scene, DateTime time, ulong frameCount, int fixedUpdateIndex, bool cursorVisible, UnityHooks.CursorLockModeType cursorLockState, UnityVersion saveVersion)
    {
        SaveVersion = saveVersion;

        Scene = scene;

        Time = time;
        FrameCount = frameCount;
        FixedUpdateIndex = fixedUpdateIndex;

        CursorVisible = cursorVisible;
        CursorLockState = cursorLockState;
    }
}
