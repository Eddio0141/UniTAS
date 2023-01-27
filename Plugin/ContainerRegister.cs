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
            c.Scan(scanner => { scanner.AddAllTypesOf<OnPluginInitProcessor>(); });

            c.For<MonoBehEventInvoker>().Singleton();
            c.For<IMonoBehEventInvoker>().Use(x => x.GetInstance<MonoBehEventInvoker>());

            c.For<StaticFieldStorage.StaticFieldStorage>().Singleton();
            c.For<IStaticFieldManipulator>().Use(x => x.GetInstance<StaticFieldStorage.StaticFieldStorage>());

            c.For<SyncFixedUpdate>().Singleton();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            c.For<SceneIndexNameTracker>().Singleton();
            c.For<ISceneIndexName>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IPluginInitialLoad>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SceneIndexNameTracker>());

            c.For<GameInfo.GameInfo>().Singleton();
            c.For<IGameInfo>().Use(x => x.GetInstance<GameInfo.GameInfo>());

            c.For<GameInitialRestart.GameInitialRestart>().Singleton();
            c.For<IGameInitialRestart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameInitialRestart.GameInitialRestart>());

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController.MonoBehaviourController>();

            c.For<SceneWrapper>().Singleton();
            c.For<ISceneWrapper>().Use(x => x.GetInstance<SceneWrapper>());

            c.For<LoadSceneParametersWrapper>().Singleton();
            c.For<ILoadSceneParametersWrapper>().Use(x => x.GetInstance<LoadSceneParametersWrapper>());

            c.For<SceneWrap>().Singleton();
            c.For<ISceneWrap>().Use(x => x.GetInstance<SceneWrap>());

            c.For<UnityWrapper>().Singleton();
            c.For<IUnityWrapper>().Use(x => x.GetInstance<UnityWrapper>());

            c.For<ObjectWrapper>().Singleton();
            c.For<IObjectWrapper>().Use(x => x.GetInstance<ObjectWrapper>());

            c.For<MonoBehaviourWrapper>().Singleton();
            c.For<IMonoBehaviourWrapper>().Use(x => x.GetInstance<MonoBehaviourWrapper>());

            c.For<Logger.Logger>().Singleton();
            c.For<ILogger>().Use(x => x.GetInstance<Logger.Logger>());

            // before FileSystemManager
            c.For<PatchReverseInvoker>().Singleton();
            c.For<IReverseInvokerFactory>().Use<ReverseInvokerFactory>();
        });

        return container;
    }

    public static void ConfigAfterInit(IContainer container)
    {
        container.Configure(c =>
        {
            c.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();

                // scanner.AddAllTypesOf(typeof(IOnUpdate));
                // scanner.AddAllTypesOf(typeof(IOnFixedUpdate));
                scanner.AddAllTypesOf<EngineExternalMethod>();
                scanner.AddAllTypesOf<PatchProcessor>();
                scanner.Exclude(type => type.IsSubclassOf(typeof(OnPluginInitProcessor)));
            });

            c.For<PluginWrapper>().Singleton();

            c.For<MovieRunner>().Singleton();
            c.For<IMovieRunner>().Use(x => x.GetInstance<MovieRunner>());
            c.For<IOnPreUpdates>().Use(x => x.GetInstance<MovieRunner>());

            c.For<GameRestart.GameRestart>().Singleton();
            c.For<IGameRestart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnAwake>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnEnable>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnStart>().Use(x => x.GetInstance<GameRestart.GameRestart>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<GameRestart.GameRestart>());

            c.For<VirtualEnvironment>().Singleton();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<VirtualEnvironment>());

            c.For<IOnPreUpdates>().Singleton().Use<GameTime>();

            c.For<IOnPreUpdates>().Use<VirtualEnvironmentApplier>();

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
    }
}