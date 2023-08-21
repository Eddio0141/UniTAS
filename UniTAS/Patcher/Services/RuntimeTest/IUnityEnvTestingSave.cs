namespace UniTAS.Patcher.Services.RuntimeTest;

/// <summary>
/// For saving and restoring some unity environment states for runtime tests
/// </summary>
public interface IUnityEnvTestingSave
{
    /// <summary>
    /// Save the current unity state
    /// </summary>
    void Save();

    /// <summary>
    /// Restore the current unity state
    /// </summary>
    void Restore();
}