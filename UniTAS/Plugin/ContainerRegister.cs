using StructureMap;
using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameEnvironment.InnerState;
using UniTAS.Plugin.GameEnvironment.InnerState.FileSystem;
using UniTAS.Plugin.GameEnvironment.InnerState.Input;
using UniTAS.Plugin.GameInfo;
using UniTAS.Plugin.GameInitialRestart;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.GameRestart.EventInterfaces;
using UniTAS.Plugin.GameRestart.Events;
using UniTAS.Plugin.GameSpeedUnlocker;
using UniTAS.Plugin.GameVideoRender;
using UniTAS.Plugin.GUI.MainMenu.Tabs;
using UniTAS.Plugin.Interfaces;
using UniTAS.Plugin.Interfaces.StartEvent;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.MainThreadSpeedController;
using UniTAS.Plugin.MonoBehaviourController;
using UniTAS.Plugin.Movie;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.EngineMethods.Implementations;
using UniTAS.Plugin.Patches.PatchProcessor;
using UniTAS.Plugin.ReverseInvoker;
using UniTAS.Plugin.StaticFieldStorage;
using UniTAS.Plugin.Trackers.AsyncSceneLoadTracker;
using UniTAS.Plugin.Trackers.SceneIndexNameTracker;
using UniTAS.Plugin.Trackers.SceneTracker;
using UniTAS.Plugin.UnitySafeWrappers;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;
using UniTAS.Plugin.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.UnitySafeWrappers.Wrappers.UnityEngine;

namespace UniTAS.Plugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.AssemblyContainingType<Plugin>();
                scanner.WithDefaultConventions();

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

            c.For<IStaticFieldManipulator>().Singleton().Use<StaticFieldStorage.StaticFieldStorage>();

            c.ForSingletonOf<SyncFixedUpdate>().Use<SyncFixedUpdate>();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            c.ForSingletonOf<SceneIndexNameTracker>().Use<SceneIndexNameTracker>();
            c.For<ISceneIndexName>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IPluginInitialLoad>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SceneIndexNameTracker>());

            c.For<IGameInfo>().Singleton().Use<GameInfo.GameInfo>();

            c.ForSingletonOf<GameInitialRestart.GameInitialRestart>().Use<GameInitialRestart.GameInitialRestart>();
            c.For<IGameInitialRestart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController.MonoBehaviourController>();

            c.For<ISceneWrapper>().Singleton().Use<SceneManagerWrapper>();

            c.For<IUnityWrapper>().Singleton().Use<UnityWrapper>();

            c.For<IRandomWrapper>().Singleton().Use<RandomWrapper>();

            c.For<ITimeWrapper>().Singleton().Use<TimeWrapper>();

            c.For<ILogger>().Singleton().Use<Logger.Logger>();
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

            c.ForSingletonOf<GameRestart.GameRestart>().Use<GameRestart.GameRestart>();
            c.For<IGameRestart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameRestart.GameRestart>());

            c.ForSingletonOf<FileSystemManager>().Use<FileSystemManager>();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<FileSystemManager>());
            c.For<IFileSystemManager>().Use(x => x.GetInstance<FileSystemManager>());

            c.ForSingletonOf<AsyncOperationTracker>().Use<AsyncOperationTracker>();
            c.For<ISceneLoadTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleCreateRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IOnLastUpdate>().Use(x => x.GetInstance<AsyncOperationTracker>());

            c.ForSingletonOf<SceneTracker>().Use<SceneTracker>();
            c.For<ISceneTracker>().Use(x => x.GetInstance<SceneTracker>());
            c.For<ILoadedSceneInfo>().Use(x => x.GetInstance<SceneTracker>());

            c.For<IUnityInstanceWrapFactory>().Singleton().Use<UnityInstanceWrapFactory>();

            c.ForSingletonOf<MainThreadSpeedControl>().Use<MainThreadSpeedControl>();
            c.For<IMainThreadSpeedControl>().Use(x => x.GetInstance<MainThreadSpeedControl>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<MainThreadSpeedControl>());

            c.ForSingletonOf<GameSpeedUnlocker.GameSpeedUnlocker>().Use<GameSpeedUnlocker.GameSpeedUnlocker>();
            c.For<IGameSpeedUnlocker>().Use(x => x.GetInstance<GameSpeedUnlocker.GameSpeedUnlocker>());

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