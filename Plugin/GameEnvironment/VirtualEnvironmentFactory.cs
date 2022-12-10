namespace UniTASPlugin.GameEnvironment;

public class VirtualEnvironmentFactory : IVirtualEnvironmentService
{
    private static VirtualEnvironment _instance = new();

    public VirtualEnvironment GetVirtualEnv()
    {
        return _instance;
    }
}