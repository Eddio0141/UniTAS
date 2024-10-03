namespace UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;

public interface IOnInputUpdateUnconditional
{
    void InputUpdateUnconditional(bool fixedUpdate, bool newInputSystemUpdate);
}