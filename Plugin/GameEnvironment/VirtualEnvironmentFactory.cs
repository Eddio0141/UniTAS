namespace UniTASPlugin.GameEnvironment;

public class VirtualEnvironmentFactory : IVirtualEnvironmentFactory
{
    private static VirtualEnvironment _instance = new();

    public VirtualEnvironment GetVirtualEnv()
    {
        return _instance;
    }
}