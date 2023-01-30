using StructureMap;
using UniTASPlugin.AsyncSceneLoadTracker;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem;
using UniTASPlugin.GameInfo;
using UniTASPlugin.GameInitialRestart;
using UniTASPlugin.GameObjectTracker;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Interfaces;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.Logger;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Patches.PatchProcessor;
using UniTASPlugin.ReverseInvoker;
using UniTASPlugin.StaticFieldStorage;
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

            c.For<PluginWrapper>().Singleton();

            c.For<IMonoBehEventInvoker>().Singleton().Use<MonoBehEventInvoker>();

            c.For<IStaticFieldManipulator>().Singleton().Use<StaticFieldStorage.StaticFieldStorage>();

            c.For<ISyncFixedUpdate>().Singleton().Use<SyncFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnUpdate>();

            c.For<SceneIndexNameTracker>().Singleton();
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

            c.For<IObjectWrapper>().Singleton().Use<ObjectWrapper>();

            c.For<IMonoBehaviourWrapper>().Singleton().Use<MonoBehaviourWrapper>();

            c.For<ILogger>().Singleton().Use<Logger.Logger>();

            // before FileSystemManager
            c.For<PatchReverseInvoker>().Singleton();
            c.For<IReverseInvokerFactory>().Use<ReverseInvokerFactory>();

            c.For<VirtualEnvironment>().Singleton();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<VirtualEnvironment>());

            c.For<VirtualEnvironmentApplier>().Singleton();
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<VirtualEnvironmentApplier>());

            c.For<MovieRunner>().Singleton();
            c.For<IMovieRunner>().Use(x => x.GetInstance<MovieRunner>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<MovieRunner>());

            c.For<GameRestart.GameRestart>().Singleton();
            c.For<IGameRestart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameRestart.GameRestart>());

            c.For<IOnPreUpdates>().Singleton().Use<GameTime>();

            c.For<FileSystemManager>().Singleton();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<FileSystemManager>());
            c.For<IFileSystemManager>().Use(x => x.GetInstance<FileSystemManager>());

            c.For<AsyncOperationTracker>().Singleton();
            c.For<ISceneLoadTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleCreateRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IAssetBundleRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            c.For<IOnLastUpdate>().Use(x => x.GetInstance<AsyncOperationTracker>());

            c.For<EndOfFrameTracker>().Singleton();
            c.For<IEndOfFrameTracker>().Use(x => x.GetInstance<EndOfFrameTracker>());

            c.For<ObjectTracker>().Singleton();
            c.For<IObjectTracker>().Use(x => x.GetInstance<ObjectTracker>());
            c.For<IObjectInfo>().Use(x => x.GetInstance<ObjectTracker>());

            c.For<SceneTracker>().Singleton();
            c.For<ISceneTracker>().Use(x => x.GetInstance<SceneTracker>());
            c.For<ILoadedSceneInfo>().Use(x => x.GetInstance<SceneTracker>());
        });

        return container;
    }
}