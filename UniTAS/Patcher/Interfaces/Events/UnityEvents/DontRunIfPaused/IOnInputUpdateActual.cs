namespace UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;

public interface IOnInputUpdateActual
{
    void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate);
}