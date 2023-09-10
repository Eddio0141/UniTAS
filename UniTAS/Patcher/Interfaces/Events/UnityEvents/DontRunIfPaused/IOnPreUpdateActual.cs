namespace UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;

public interface IOnPreUpdateActual
{
    /// <summary>
    /// Invokes before `MonoBehaviour.Update()` and `MonoBehaviour.FixedUpdate()` is called
    /// It will always call before either of those methods
    /// </summary>
    void PreUpdateActual();
}