namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

/// <summary>
/// Interface for updating button state internally from the axis information.
/// </summary>
public interface IAxisButtonStateEnvUpdate
{
    void Hold(string button);
    void Release(string button);
    void FlushBufferedInputs();
    void Update();
    void ResetState();
}