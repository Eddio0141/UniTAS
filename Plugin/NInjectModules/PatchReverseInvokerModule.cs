using Ninject.Modules;

namespace UniTASPlugin.NInjectModules;

public class PatchReverseInvokerModule : NinjectModule
{
    public override void Load()
    {
        Bind<PatchReverseInvoker>().ToSelf().InSingletonScope();
    }
}