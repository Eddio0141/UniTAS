namespace UniTAS.Patcher.Interfaces.Events;

public interface IOnVirtualEnvStatusChange
{
    void OnVirtualEnvStatusChange(bool runVirtualEnv);
}