namespace UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;

public interface IOnInputUpdateUnconditional
{
    void InputUpdateUnconditional(bool fixedUpdate, bool newInputSystemUpdate);
}