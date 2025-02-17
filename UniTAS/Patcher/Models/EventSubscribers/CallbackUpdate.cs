namespace UniTAS.Patcher.Models.EventSubscribers;

public enum CallbackUpdate
{
    AwakeUnconditional,
    AwakeActual,
    StartUnconditional,
    StartActual,
    EnableUnconditional,
    EnableActual,
    UpdateUnconditional,
    UpdateActual,
    FixedUpdateUnconditional,
    FixedUpdateActual,
    GUIUnconditional,
    GUIActual,
    LateUpdateUnconditional,
    LateUpdateActual,
    EndOfFrameActual,
    LastUpdateUnconditional,
    LastUpdateActual
}