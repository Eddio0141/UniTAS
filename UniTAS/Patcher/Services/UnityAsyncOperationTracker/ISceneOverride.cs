namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface ISceneOverride
{
    /// <summary>
    /// Returns true if override loaded state
    /// </summary>
    bool IsLoaded(int handle, out bool loaded);
}