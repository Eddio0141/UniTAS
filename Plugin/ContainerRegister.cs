using StructureMap;
using UniTASPlugin.AsyncSceneLoadTracker;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.GameEnvironment.InnerState.FileSystem;
using UniTASPlugin.GameObjectTracker;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Interfaces;
using UniTASPlugin.Interfaces.StartEvent;
using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.MonoBehaviourController;
using UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Patches.PatchProcessor;
using UniTASPlugin.ReverseInvoker;
using UniTASPlugin.Trackers.SceneIndexNameTracker;
using UniTASPlugin.Trackers.SceneTracker;

namespace UniTASPlugin;

public static class ContainerRegister
{
    public static Container Init()
    {
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.WithDefaultConventions();

                // scanner.AddAllTypesOf(typeof(IOnUpdate));
                // scanner.AddAllTypesOf(typeof(IOnFixedUpdate));
                scanner.AddAllTypesOf<EngineExternalMethod>();
                scanner.AddAllTypesOf<PatchProcessor>();
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

            c.For<SyncFixedUpdate>().Singleton();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            // before FileSystemManager
            c.For<PatchReverseInvoker>().Singleton();

            c.For<VirtualEnvironment>().Singleton();
            c.For<IOnGameRestart>().Use(x => x.GetInstance<VirtualEnvironment>());

            c.For<IOnPreUpdates>().Singleton().Use<GameTime>();

            c.For<IOnPreUpdates>().Use<VirtualEnvironmentApplier>();

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController.MonoBehaviourController>();

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

            // note: this code on initial load needs to run before destroying game objects
            c.For<SceneIndexNameTracker>().Singleton();
            c.For<ISceneIndexName>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IPluginInitialLoad>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SceneIndexNameTracker>());

            c.For<SceneTracker>().Singleton();
            c.For<ISceneTracker>().Use(x => x.GetInstance<SceneTracker>());
            c.For<ILoadedSceneInfo>().Use(x => x.GetInstance<SceneTracker>());
        });

        return container;
    }
}