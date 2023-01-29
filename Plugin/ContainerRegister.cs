using System;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
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
    private class PatchProcessorPluginInit : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(OnPluginInitProcessor)))
            {
                registry.For(typeof(OnPluginInitProcessor)).Add(type);
            }
        }
    }

    public static Container Init()
    {
        // we load the minimal requirements to run the plugin at setup
        var container = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.TheCallingAssembly();
                scanner.Convention<PatchProcessorPluginInit>();
            });

            c.For<IMonoBehEventInvoker>().Singleton().Use<MonoBehEventInvoker>();

            c.For<IStaticFieldManipulator>().Singleton().Use<StaticFieldStorage.StaticFieldStorage>();

            c.For<ISyncFixedUpdate>().Singleton().Use<SyncFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnFixedUpdate>();
            c.Forward<ISyncFixedUpdate, IOnUpdate>();

            c.For<ISceneIndexName>().Singleton().Use<SceneIndexNameTracker>();
            c.Forward<ISceneIndexName, IPluginInitialLoad>();
            c.Forward<ISceneIndexName, IOnUpdate>();

            c.For<IGameInfo>().Singleton().Use<GameInfo.GameInfo>();

            c.For<IGameInitialRestart>().Singleton().Use<GameInitialRestart.GameInitialRestart>();
            c.Forward<IGameInitialRestart, IOnAwake>();
            c.Forward<IGameInitialRestart, IOnEnable>();
            c.Forward<IGameInitialRestart, IOnStart>();
            c.Forward<IGameInitialRestart, IOnFixedUpdate>();

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

            c.For<IVirtualEnvironmentFactory>().Use<VirtualEnvironmentFactory>();
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

                // exclude all from first register
                scanner.ExcludeType<IMonoBehEventInvoker>();
                scanner.ExcludeType<IStaticFieldManipulator>();
                scanner.ExcludeType<ISyncFixedUpdate>();
                scanner.ExcludeType<ISceneIndexName>();
                scanner.ExcludeType<IGameInfo>();
                scanner.ExcludeType<IGameInitialRestart>();
                scanner.ExcludeType<IMonoBehaviourController>();
                scanner.ExcludeType<ISceneWrapper>();
                scanner.ExcludeType<ILoadSceneParametersWrapper>();
                scanner.ExcludeType<ISceneWrap>();
                scanner.ExcludeType<IUnityWrapper>();
                scanner.ExcludeType<IObjectWrapper>();
                scanner.ExcludeType<IMonoBehaviourWrapper>();
                scanner.ExcludeType<ILogger>();
            });

            c.For<PluginWrapper>().Singleton();

            c.For<IMovieRunner>().Singleton().Use<MovieRunner>();
            c.Forward<IMovieRunner, IOnPreUpdates>();

            c.For<IGameRestart>().Singleton().Use<GameRestart.GameRestart>();
            c.Forward<IGameRestart, IOnAwake>();
            c.Forward<IGameRestart, IOnEnable>();
            c.Forward<IGameRestart, IOnStart>();
            c.Forward<IGameRestart, IOnFixedUpdate>();

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

            // re-register MonoBehEventInvoker with new instance
            c.For<IMonoBehEventInvoker>().Singleton().Use<MonoBehEventInvoker>();
        });

        container.GetInstance<IMonoBehEventInvoker>();
    }
}