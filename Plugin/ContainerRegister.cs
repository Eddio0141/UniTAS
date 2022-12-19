using StructureMap;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.MovieRunner;
using UniTASPlugin.Movie.MovieRunner.EngineMethods;
using UniTASPlugin.Movie.MovieRunner.ParseInterfaces;
using UniTASPlugin.Movie.MovieRunner.Parsers;
using UniTASPlugin.Movie.MovieRunner.Parsers.MoviePropertyParser;
using UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser;
using UniTASPlugin.ReverseInvoker;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container();

        container.Configure(_ => _.For<PluginWrapper>().Singleton());

        // priority
        FixedUpdateSyncRegisters(container);

        GameRestartRegisters(container);
        MovieEngineRegisters(container);
        VirtualEnvRegisters(container);
        ReverseInvokerRegisters(container);

        return container;
    }

    private static void GameRestartRegisters(IContainer container)
    {
        container.Configure(_ => { _.For<IGameRestart>().Singleton().Use<GameRestart.GameRestart>(); });
    }

    private static void FixedUpdateSyncRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            _.For<FixedUpdateTracker>().Singleton();
            _.For<IOnUpdate>().Use(c => c.GetInstance<FixedUpdateTracker>());
            _.For<IOnFixedUpdate>().Use(c => c.GetInstance<FixedUpdateTracker>());
            _.For<ISyncFixedUpdate>().Use(c => c.GetInstance<FixedUpdateTracker>());
        });
    }

    private static void ReverseInvokerRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            _.For<IReverseInvokerFactory>().Use<ReverseInvokerFactory>();
            _.For<PatchReverseInvoker>().Singleton();
        });
    }

    private static void MovieEngineRegisters(IContainer container)
    {
        container.Configure(_ =>
        {
            // parser binds
            _.For<IMoviePropertyParser>().Use<MoviePropertyParser>();
            _.For<IMovieScriptParser>().Use<MovieScriptParser>();
            _.For<IMovieSectionSplitter>().Use<DefaultMovieSectionSplitter>();
            _.For<IMovieParser>().Use<MovieParser>();

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