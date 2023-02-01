namespace UniTASPlugin.Interfaces;

public interface IPluginInitialLoad
{
    void OnInitialLoad();
    bool FinishedOperation { get; }
}