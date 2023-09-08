using System;

namespace UniTAS.Patcher.Implementations.FrameAdvancing;

public partial class FrameAdvancing
{
    private struct PendingFrameAdvance
    {
        public readonly uint PendingFrames;
        public readonly FrameAdvanceMode FrameAdvanceMode;

        public PendingFrameAdvance(uint pendingFrames, FrameAdvanceMode frameAdvanceMode)
        {
            PendingFrames = pendingFrames;
            FrameAdvanceMode = frameAdvanceMode;
        }
    }

    private enum PendingUpdateOffsetFixState
    {
        // check update offset if its invalid after FixedUpdate
        PendingCheckUpdateOffset,
        PendingSync,
        Done
    }
}

[Flags]
public enum FrameAdvanceMode
{
    Update = 1,
    FixedUpdate = 2
}