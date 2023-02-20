namespace UniTAS.Plugin.Interfaces.Update;

/// <summary>
/// Called once before other script's Update / FixedUpdate is called
/// </summary>
public interface IOnPreUpdates
{
    void PreUpdate();
}