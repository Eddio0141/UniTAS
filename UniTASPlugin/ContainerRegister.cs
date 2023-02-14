using StructureMap;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.GameInfo;
using UniTASPlugin.GameInitialRestart;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Interfaces;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Patches.PatchProcessor;
using UniTASPlugin.ReverseInvoker;
using UniTASPlugin.StaticFieldStorage;
using UniTASPlugin.Trackers.AsyncSceneLoadTracker;
using UniTASPlugin.Trackers.DontDestroyOnLoadTracker;
using UniTASPlugin.Trackers.SceneIndexNameTracker;
using UniTASPlugin.Trackers.SceneTracker;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;
using UniTASPlugin.UnitySafeWrappers.Wrappers;
using UniTASPlugin.UnitySafeWrappers.Wrappers.SceneManagement;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        // we load the minimal requirements to run the plugin at setup
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();

                scanner.AddAllTypesOf<EngineExternalMethod>();
                scanner.AddAllTypesOf<PatchProcessor>();
            });

            c.ForSingletonOf<PluginWrapper>().Use<PluginWrapper>();

            c.For<IMonoBehEventInvoker>().Singleton().Use<MonoBehEventInvoker>();

            c.For<IStaticFieldManipulator>().Singleton().Use<StaticFieldStorage.StaticFieldStorage>();

            c.For<ISyncFixedUpdate>().Singleton().Use<SyncFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnUpdate>();

            c.ForSingletonOf<SceneIndexNameTracker>().Use<SceneIndexNameTracker>();
            c.For<ISceneIndexName>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IPluginInitialLoad>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SceneIndexNameTracker>());

            c.For<IGameInfo>().Singleton().Use<GameInfo.GameInfo>();

            c.For<GameInitialRestart.GameInitialRestart>().Singleton();
            c.For<IGameInitialRestart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController.MonoBehaviourController>();

            c.For<ISceneWrapper>().Singleton().Use<SceneWrapper>();

            c.For<ILoadSceneParametersWrapper>().Singleton().Use<LoadSceneParametersWrapper>();

            c.For<ISceneWrap>().Singleton().Use<SceneWrap>();

            c.For<IUnityWrapper>().Singleton().Use<UnityWrapper>();

            c.For<IRandomWrapper>().Singleton().Use<RandomWrapper>();

            c.For<ITimeWrapper>().Singleton().Use<TimeWrapper>();

            c.For<ILogger>().Singleton().Use<Logger.Logger>();

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

            c.ForSingletonOf<MovieRunner>().Use<MovieRunner>();
            c.For<IMovieRunner>().Use(x => x.GetInstance<MovieRunner>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<MovieRunner>());

            c.For<GameRestart.GameRestart>().Singleton();
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

            c.ForSingletonOf<DontDestroyOnLoadTracker>().Use<DontDestroyOnLoadTracker>();
            c.For<IDontDestroyOnLoadTracker>().Use(x => x.GetInstance<DontDestroyOnLoadTracker>());
            c.For<IDontDestroyOnLoadInfo>().Use(x => x.GetInstance<DontDestroyOnLoadTracker>());
        });

        return container;
    }
}