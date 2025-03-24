namespace UniTAS.Patcher.Implementations.FrameAdvancing;

public partial class FrameAdvancing
{
    private struct PendingFrameAdvance(uint pendingFrames)
    {
        public readonly uint PendingFrames = pendingFrames;
    }

    private enum PendingUpdateOffsetFixState
    {
        // check update offset if its invalid after FixedUpdate
        PendingCheckUpdateOffset,
        PendingSync,
        Done
    }
}