using System;

namespace UniTASPlugin.LegacySaveState;

public class State
{
    // scene
    // TODO sort out depending on unity version
    //public readonly int Scene;
    // time stuff
    public readonly DateTime Time;
    public readonly ulong FrameCount;

    public readonly int FixedUpdateIndex;
    // cursor stuff
    // TODO sort out depending on unity version
    //public readonly bool CursorVisible;
    //public readonly UnityHooks.CursorLockModeType CursorLockState;

    public State( /*int scene,*/ DateTime time, ulong frameCount,
        int fixedUpdateIndex /*bool cursorVisible, UnityHooks.CursorLockModeType cursorLockState,*/)
    {
        // TODO sort out depending on unity version
        //Scene = scene;

        Time = time;
        FrameCount = frameCount;
        FixedUpdateIndex = fixedUpdateIndex;

        //CursorVisible = cursorVisible;
        //CursorLockState = cursorLockState;
    }
}