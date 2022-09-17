using System;

namespace Core.SaveState;

internal class State
{
    // scene
    public readonly int Scene;
    // time stuff
    public readonly DateTime Time;
    public readonly ulong FrameCount;
    public readonly int FixedUpdateIndex;

    public State(int scene, DateTime time, ulong frameCount, int fixedUpdateIndex)
    {
        Scene = scene;
        Time = time;
        FrameCount = frameCount;
        FixedUpdateIndex = fixedUpdateIndex;
    }
}
