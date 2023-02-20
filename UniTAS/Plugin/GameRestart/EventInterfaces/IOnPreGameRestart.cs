namespace UniTAS.Plugin.GameRestart.EventInterfaces;

/// <summary>
/// Interface for classes that need to be notified when the game is about to be restarted
/// </summary>
public interface IOnPreGameRestart
{
    void OnPreGameRestart();
}