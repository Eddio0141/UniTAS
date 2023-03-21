namespace UniTAS.Plugin.Interfaces.Events;

public interface IPluginInitialLoad
{
    void OnInitialLoad();
    bool FinishedOperation { get; }
}