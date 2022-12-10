using UniTASFunkyInjector;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using UniTASPlugin.UpdateHelper;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static FunkyInjectorContainer Init()
    {
        var container = new FunkyInjectorContainer();

        MovieEngineRegisters(container);
        VirtualEnvRegisters(container);
        PatchReverseInvokerRegisters(container);
        OnUpdateRegisters(container);
        ReverseInvokerRegisters(container);

        return container;
    }

    private static void ReverseInvokerRegisters(FunkyInjectorContainer container)
    {
        container.Register(ComponentStarter.For<IReverseInvokerService>().ImplementedBy<ReverseInvokerFactory>());
    }

    private static void OnUpdateRegisters(FunkyInjectorContainer container)
    {
        container.Register(ComponentStarter.For<IOnUpdate>().ImplementedBy<MouseState>());
        container.Register(ComponentStarter.For<IOnUpdate>().ImplementedBy<AxisState>());
        container.Register(ComponentStarter.For<IOnUpdate>().ImplementedBy<KeyboardState>());
    }

    private static void PatchReverseInvokerRegisters(FunkyInjectorContainer container)
    {
        container.Register(ComponentStarter.For<PatchReverseInvoker>().LifestyleSingleton());
    }

    private static void MovieEngineRegisters(FunkyInjectorContainer container)
    {
        // parser binds
        container.Register(ComponentStarter.For<IMoviePropertyParser>().ImplementedBy<DefaultMoviePropertiesParser>());
        container.Register(ComponentStarter.For<IMovieScriptParser>().ImplementedBy<DefaultMovieScriptParser>());
        container.Register(ComponentStarter.For<IMovieSectionSplitter>().ImplementedBy<DefaultMovieSectionSplitter>());
        container.Register(ComponentStarter.For<IMovieParser>().ImplementedBy<ScriptEngineMovieParser>());

        // runner binds
        container.Register(ComponentStarter.For<IMovieRunner>().ImplementedBy<ScriptEngineMovieRunner>()
            .LifestyleSingleton());

        // extern method binds
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<PrintExternalMethod>());

        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<RegisterExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<UnregisterExternalMethod>());

        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<HoldKeyExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<UnHoldKeyExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<ClearHeldKeysExternalMethod>());

        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<MoveMouseExternalMethod>());

        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<SetFpsExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<SetFrameTimeExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<GetFpsExternalMethod>());
        container.Register(ComponentStarter.For<EngineExternalMethod>().ImplementedBy<GetFrameTimeExternalMethod>());
    }

    private static void VirtualEnvRegisters(FunkyInjectorContainer container)
    {
        container.Register(
            ComponentStarter.For<IVirtualEnvironmentService>().ImplementedBy<VirtualEnvironmentFactory>());
        container.Register(ComponentStarter.For<IOnUpdate>().ImplementedBy<VirtualEnvironmentApplier>());
    }
}