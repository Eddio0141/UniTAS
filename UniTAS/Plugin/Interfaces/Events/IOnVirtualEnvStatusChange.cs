namespace UniTAS.Plugin.Interfaces.Events;

public interface IOnVirtualEnvStatusChange
{
    void OnVirtualEnvStatusChange(bool runVirtualEnv);
}