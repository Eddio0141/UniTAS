namespace UniTAS.Plugin.Interfaces.Events.SoftRestart;

/// <summary>
/// Interface for classes that need to be notified when the game is about to be restarted
/// </summary>
public interface IOnPreGameRestart
{
    void OnPreGameRestart();
}