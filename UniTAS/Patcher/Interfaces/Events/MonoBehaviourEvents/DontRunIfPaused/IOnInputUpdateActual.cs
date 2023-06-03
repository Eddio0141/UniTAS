namespace UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;

public interface IOnInputUpdateActual
{
    void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate);
}