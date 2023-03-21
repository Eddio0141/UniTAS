namespace UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;

/// <summary>
/// Called once before other script's Update / FixedUpdate is called
/// </summary>
public interface IOnPreUpdates
{
    void PreUpdate();
}