namespace UniTAS.Patcher.Services.InputSystemOverride;

public interface IInputSystemState
{
    bool HasNewInputSystem { get; }
    bool HasOldInputSystem { get; }
    bool HasRewired { get; }
}