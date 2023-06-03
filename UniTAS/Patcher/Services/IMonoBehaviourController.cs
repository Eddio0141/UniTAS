namespace UniTAS.Patcher.Services;

public interface IMonoBehaviourController
{
    bool PausedExecution { get; set; }
    bool PausedUpdate { get; set; }
}