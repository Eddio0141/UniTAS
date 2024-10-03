namespace UniTAS.Patcher.Services.GameExecutionControllers;

public interface IFinalizeSuppressor
{
    bool DisableFinalizeInvoke { get; set; }
}