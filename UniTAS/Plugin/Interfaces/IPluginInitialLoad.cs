namespace UniTAS.Plugin.Interfaces;

public interface IPluginInitialLoad
{
    void OnInitialLoad();
    bool FinishedOperation { get; }
}