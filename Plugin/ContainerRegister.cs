using StructureMap;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container();

        FixedUpdateSyncRegisters(container);
        GameRestartRegisters(container);
        MovieEngineRegisters(container);
        VirtualEnvRegisters(container);
        OnUpdateRegisters(container);
        ReverseInvokerRegisters(container);

        return container;
    }

    private static void GameRestartRegisters(IContainer container)
    {
        container.Configure(_ => { _.For<IGameRestart>().Singleton().Use<GameRestart>(); });
    }

    private static void FixedUpdateSyncRegisters(IContainer container)
    {
        container.Configure(_ => { _.For<ISyncFixedUpdate>().Singleton().Use<FixedUpdateTracker>(); });
    }

    private static void ReverseInvokerRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            _.For<IReverseInvokerFactory>().Use<ReverseInvokerFactory>();
            _.For<PatchReverseInvoker>().Singleton();
        });
    }

    private static void OnUpdateRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            // priority
            _.For<IOnUpdate>().Use<FixedUpdateTracker>();
            _.For<IOnFixedUpdate>().Use<FixedUpdateTracker>();

            _.For<IOnUpdate>().Use<MouseState>();
            _.For<IOnUpdate>().Use<AxisState>();
            _.For<IOnUpdate>().Use<KeyboardState>();
        });
    }

    private static void MovieEngineRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            // parser binds
            _.For<IMoviePropertyParser>().Use<DefaultMoviePropertiesParser>();
            _.For<IMovieScriptParser>().Use<DefaultMovieScriptParser>();
            _.For<IMovieSectionSplitter>().Use<DefaultMovieSectionSplitter>();
            _.For<IMovieParser>().Use<ScriptEngineMovieParser>();

            // runner binds
            _.For<IMovieRunner>().Singleton().Use<ScriptEngineMovieRunner>();

            // extern method binds
            _.For<EngineExternalMethod>().Use<PrintExternalMethod>();

            _.For<EngineExternalMethod>().Use<RegisterExternalMethod>();
            _.For<EngineExternalMethod>().Use<UnregisterExternalMethod>();

            _.For<EngineExternalMethod>().Use<HoldKeyExternalMethod>();
            _.For<EngineExternalMethod>().Use<UnHoldKeyExternalMethod>();
            _.For<EngineExternalMethod>().Use<ClearHeldKeysExternalMethod>();

            _.For<EngineExternalMethod>().Use<MoveMouseExternalMethod>();

            _.For<EngineExternalMethod>().Use<SetFpsExternalMethod>();
            _.For<EngineExternalMethod>().Use<SetFrameTimeExternalMethod>();
            _.For<EngineExternalMethod>().Use<GetFpsExternalMethod>();
            _.For<EngineExternalMethod>().Use<GetFrameTimeExternalMethod>();
        });
    }

    private static void VirtualEnvRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            _.For<IVirtualEnvironmentFactory>().Use<VirtualEnvironmentFactory>();
            _.For<VirtualEnvironment>().Singleton();
            _.For<IOnUpdate>().Use<VirtualEnvironmentApplier>();
        });
    }
}