namespace UniTAS.Patcher.Models.EventSubscribers;

public enum CallbackPriority
{
    // the higher you place it, the lower the value, the earlier it gets invoked
    InputUpdate,
    UpdateInvokeOffset,
    FirstUpdateSkipOnRestart, // shouldn't matter if its before UpdateInvokeOffset, since it just handles Actual calls
    Default,
    GameRestart,
    UnityRuntimeInitAttributeInvoker, // after GameRestart (Default in this case)
    FrameAdvancingTest,
    FirstUpdateSkipOnRestartLastUpdate, // need for last update resuming
}