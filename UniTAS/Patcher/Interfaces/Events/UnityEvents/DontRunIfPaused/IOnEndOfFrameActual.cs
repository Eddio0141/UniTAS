namespace UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;

/// <summary>
/// End of frame, not the last update
/// This simply runs code on start of `yield WaitForEndOfFrame`
/// </summary>
public interface IOnEndOfFrameActual
{
    void OnEndOfFrame();
}