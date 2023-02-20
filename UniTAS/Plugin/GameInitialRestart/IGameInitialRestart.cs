namespace UniTAS.Plugin.GameInitialRestart;

public interface IGameInitialRestart
{
    void InitialRestart();
    bool FinishedRestart { get; }
}