using StructureMap;

namespace UniTASPlugin.ReverseInvoker;

public class ReverseInvokerFactory : IReverseInvokerFactory
{
    private readonly IContainer _container;

    public ReverseInvokerFactory(IContainer container)
    {
        _container = container;
    }

    public PatchReverseInvoker GetReverseInvoker()
    {
        return _container.GetInstance<PatchReverseInvoker>();
    }
}