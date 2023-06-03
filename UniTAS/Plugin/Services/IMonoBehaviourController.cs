namespace UniTAS.Plugin.Services;

public interface IMonoBehaviourController
{
    bool PausedExecution { get; set; }
    bool PausedUpdate { get; set; }
}