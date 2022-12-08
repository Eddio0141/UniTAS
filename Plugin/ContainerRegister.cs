using Castle.Facilities.TypedFactory;
using Castle.Windsor;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using UniTASPlugin.UpdateHelper;
using Component = Castle.MicroKernel.Registration.Component;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static IWindsorContainer Init()
    {
        var container = new WindsorContainer();

        container.AddFacility<TypedFactoryFacility>();

        MovieEngineRegisters(container);
        VirtualEnvRegisters(container);
        PatchReverseInvokerRegisters(container);
        OnUpdateRegisters(container);

        return container;
    }

    private static void OnUpdateRegisters(IWindsorContainer container)
    {
        container.Register(Component.For<IOnUpdate>().ImplementedBy<MouseState>());
        container.Register(Component.For<IOnUpdate>().ImplementedBy<AxisState>());
        container.Register(Component.For<IOnUpdate>().ImplementedBy<KeyboardState>());
    }

    private static void PatchReverseInvokerRegisters(IWindsorContainer container)
    {
        container.Register(Component.For<PatchReverseInvoker>().LifestyleSingleton());
    }

    private static void MovieEngineRegisters(IWindsorContainer container)
    {
        // parser binds
        container.Register(Component.For<IMoviePropertyParser>().ImplementedBy<DefaultMoviePropertiesParser>());
        container.Register(Component.For<IMovieScriptParser>().ImplementedBy<DefaultMovieScriptParser>());
        container.Register(Component.For<IMovieSectionSplitter>().ImplementedBy<DefaultMovieSectionSplitter>());
        container.Register(Component.For<IMovieParser>().ImplementedBy<ScriptEngineMovieParser>());

        // runner binds
        container.Register(Component.For<IMovieRunner>().ImplementedBy<ScriptEngineMovieRunner>().LifestyleSingleton());

        // extern method binds
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<PrintExternalMethod>());

        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<RegisterExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<UnregisterExternalMethod>());

        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<HoldKeyExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<UnHoldKeyExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<ClearHeldKeysExternalMethod>());

        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<MoveMouseExternalMethod>());

        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<SetFpsExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<SetFrameTimeExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<GetFpsExternalMethod>());
        container.Register(Component.For<EngineExternalMethod>().ImplementedBy<GetFrameTimeExternalMethod>());
    }

    private static void VirtualEnvRegisters(IWindsorContainer container)
    {
        container.Register(Component.For<IVirtualEnvironmentService>().AsFactory());
    }
}