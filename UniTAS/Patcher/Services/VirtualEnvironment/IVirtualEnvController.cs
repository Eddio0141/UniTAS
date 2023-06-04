namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface IVirtualEnvController
{
    bool RunVirtualEnvironment { get; set; }
    event VirtualEnvStatusChange OnVirtualEnvStatusChange;
}

public delegate void VirtualEnvStatusChange(bool runVirtualEnv);