namespace UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;

/// <summary>
/// Called once before other script's Update / FixedUpdate is called
/// </summary>
public interface IOnPreUpdatesUnconditional
{
    void PreUpdateUnconditional();
}