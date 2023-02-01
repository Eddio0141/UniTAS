namespace UniTASPlugin.GameInitialRestart;

public interface IGameInitialRestart
{
    void InitialRestart();
    bool FinishedRestart { get; }
}