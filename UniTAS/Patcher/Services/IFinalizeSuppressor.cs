namespace UniTAS.Patcher.Services;

public interface IFinalizeSuppressor
{
    bool DisableFinalizeInvoke { set; }
}