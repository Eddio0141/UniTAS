namespace UniTAS.Patcher.Services.GameExecutionControllers;

public interface IMonoBehaviourController
{
    bool PausedExecution { get; set; }
}