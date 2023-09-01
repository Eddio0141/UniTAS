namespace UniTAS.Patcher.Models.EventSubscribers;

public enum CallbackPriority
{
    // the higher you place it, the lower the value, the earlier it gets invoked
    FirstUpdateSkipOnRestart,
    Default,
    FrameAdvancingTest
}