using Ninject.Modules;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.UpdateHelper;

namespace UniTASPlugin.NInjectModules;

public class OnUpdateModule : NinjectModule
{
    public override void Load()
    {
        Bind<IOnUpdate>().To<MouseState>();
        Bind<IOnUpdate>().To<AxisState>();
        Bind<IOnUpdate>().To<KeyboardState>();
    }
}