using StructureMap;
using UniTAS.Plugin.Implementations;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Implementations.GameRestart;
using UniTAS.Plugin.Implementations.Logging;
using UniTAS.Plugin.Implementations.Movie;
using UniTAS.Plugin.Implementations.Movie.Engine.Modules;
using UniTAS.Plugin.Implementations.UnitySafeWrappers;
using UniTAS.Plugin.Implementations.UnitySafeWrappers.UnityEngine;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Interfaces.GUI;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Interfaces.Patches.PatchProcessor;
using UniTAS.Plugin.Interfaces.TASRenderer;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.EventSubscribers;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.UnityAsyncOperationTracker;
using UniTAS.Plugin.Services.UnitySafeWrappers;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.FileSystem;
using UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

namespace UniTAS.Plugin.Utils;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.AssemblyContainingType<Plugin>();
                scanner.Convention<DependencyInjectionConvention>();

                scanner.AddAllTypesOf<PatchProcessor>();
                scanner.AddAllTypesOf<IMainMenuTab>();
                scanner.AddAllTypesOf<EngineMethodClass>();
                scanner.AddAllTypesOf<VideoRenderer>();
                scanner.AddAllTypesOf<AudioRenderer>();
                scanner.ExcludeType<Env>();
            });

            c.ForSingletonOf<PluginWrapper>().Use<PluginWrapper>();

            c.ForSingletonOf<MonoBehEventInvoker>().Use<MonoBehEventInvoker>();
            c.For<IMonoBehEventInvoker>().Use(x => x.GetInstance<MonoBehEventInvoker>());
            c.For<IUpdateEvents>().Use(x => x.GetInstance<MonoBehEventInvoker>());

            c.For<IStaticFieldManipulator>().Singleton().Use<StaticFieldStorage>();

            c.ForSingletonOf<SyncFixedUpdate>().Use<SyncFixedUpdate>();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            c.For<IGameInfo>().Singleton().Use<GameInfo>();

            c.ForSingletonOf<GameInitialRestart>().Use<GameInitialRestart>();
            c.For<IGameInitialRestart>().Use(x => x.GetInstance<GameInitialRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameInitialRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameInitialRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameInitialRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameInitialRestart>());

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController>();

            c.For<ISceneWrapper>().Singleton().Use<SceneManagerWrapper>();

            c.For<IUnityWrapper>().Singleton().Use<UnityWrapper>();

            c.For<IRandomWrapper>().Singleton().Use<RandomWrapper>();

            c.For<ITimeWrapper>().Singleton().Use<TimeWrapper>();

            c.For<ILogger>().Singleton().Use<Logger>();
            c.For<IMovieLogger>().Singleton().Use<MovieLogger>();

            // before FileSystemManager
            c.ForSingletonOf<PatchReverseInvoker>().Use<PatchReverseInvoker>();
            c.For<IPatchReverseInvoker>().Use(x => x.GetInstance<PatchReverseInvoker>());

            // before VirtualEnvironment
            c.ForSingletonOf<GameTime>().Use<GameTime>();
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<GameTime>());
            c.For<IOnGameRestartResume>().Use(x => x.GetInstance<GameTime>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameTime>());

            // before VirtualEnvironment
            c.ForSingletonOf<VirtualEnvironmentApplier>().Use<VirtualEnvironmentApplier>();
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<VirtualEnvironmentApplier>());

            // after VirtualEnvironmentApplier
            c.ForSingletonOf<InputState>().Use<InputState>();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<InputState>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<InputState>());

            c.ForSingletonOf<VirtualEnvironment>().Use<VirtualEnvironment>();
            c.For<IOnGameRestartResume>().Use(x => x.GetInstance<VirtualEnvironment>());

            // after VirtualEnvironment
            c.For<IOnGameRestartResume>().Use<UnityRngRestartInit>();

            c.ForSingletonOf<MovieRunner>().Use<MovieRunner>();
            c.For<IMovieRunner>().Use(x => x.GetInstance<MovieRunner>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<MovieRunner>());

            c.ForSingletonOf<GameRestart>().Use<GameRestart>();
            c.For<IGameRestart>().Use(x => x.GetInstance<GameRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameRestart>());

            c.ForSingletonOf<FileSystemManager>().Use<FileSystemManager>();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<FileSystemManager>());
            c.For<IFileSystemManager>().Use(x => x.GetInstance<FileSystemManager>());

            c.ForSingletonOf<AsyncOperationTracker>().Use<AsyncOperationTracker>();
            c.For<ISceneLoadTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleCreateRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IOnLastUpdate>().Use(x => x.GetInstance<AsyncOperationTracker>());

            c.For<IUnityInstanceWrapFactory>().Singleton().Use<UnityInstanceWrapFactory>();

            c.ForSingletonOf<MainThreadSpeedControl>().Use<MainThreadSpeedControl>();
            c.For<IMainThreadSpeedControl>().Use(x => x.GetInstance<MainThreadSpeedControl>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<MainThreadSpeedControl>());

            c.ForSingletonOf<GameSpeedUnlocker>().Use<GameSpeedUnlocker>();
            c.For<IGameSpeedUnlocker>().Use(x => x.GetInstance<GameSpeedUnlocker>());

            c.ForSingletonOf<Env>().Use<Env>();
            c.For<EngineMethodClass>().Use(x => x.GetInstance<Env>());
            c.For<IOnLastUpdate>().Use(x => x.GetInstance<Env>());

            c.ForSingletonOf<GameRender>().Use<GameRender>();
            c.For<IGameRender>().Use(x => x.GetInstance<GameRender>());
            c.For<IOnLastUpdate>().Use(x => x.GetInstance<GameRender>());

            c.ForSingletonOf<AudioRendererWrapper>().Use<AudioRendererWrapper>();
            c.For<IAudioRendererWrapper>().Use(x => x.GetInstance<AudioRendererWrapper>());
        });

        return container;
    }
}