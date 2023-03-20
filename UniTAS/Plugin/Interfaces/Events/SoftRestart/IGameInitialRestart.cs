namespace UniTAS.Plugin.Interfaces.Events.SoftRestart;

public interface IGameInitialRestart
{
    void InitialRestart();
    bool FinishedRestart { get; }
}