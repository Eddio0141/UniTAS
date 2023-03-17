using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameInitialRestart;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.GameRestart.EventInterfaces;
using UniTAS.Plugin.GameVideoRender;
using UniTAS.Plugin.Interfaces.StartEvent;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.MonoBehaviourController;
using UniTAS.Plugin.Movie;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.Parsers.MovieParser;
using UniTAS.Plugin.ReverseInvoker;
using UniTAS.Plugin.StaticFieldStorage;
using UniTAS.Plugin.UnitySafeWrappers;
using UniTAS.Plugin.UnitySafeWrappers.Interfaces;
using UniTAS.Plugin.UnitySafeWrappers.Wrappers;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public static class KernelUtils
{
    public class Env : EngineMethodClass, IOnLastUpdate
    {
        public float Fps { get; set; }
        public float Frametime { get; set; }

        [MoonSharpHidden]
        public void OnLastUpdate()
        {
        }
    }

    public class DummyLogger : IMovieLogger
    {
        public List<string> Infos { get; } = new();
        public List<string> Warns { get; } = new();
        public List<string> Errors { get; } = new();

        public void LogError(object data)
        {
            Errors.Add(data.ToString() ?? string.Empty);
        }

        public void LogInfo(object data)
        {
            Infos.Add(data.ToString() ?? string.Empty);
        }

        public void LogWarning(object data)
        {
            Warns.Add(data.ToString() ?? string.Empty);
        }

#pragma warning disable 67
        public event EventHandler<LogEventArgs>? OnLog;
#pragma warning restore 67
    }

    private class FakeLogger : ILogger
    {
        public void LogFatal(object data)
        {
        }

        public void LogError(object data)
        {
        }

        public void LogWarning(object data)
        {
        }

        public void LogMessage(object data)
        {
        }

        public void LogInfo(object data)
        {
        }

        public void LogDebug(object data)
        {
        }
    }

    private class FakeStaticFieldStorage : IStaticFieldManipulator
    {
        public void ResetStaticFields()
        {
        }
    }

    public static Container Init()
    {
        var kernel = new Container(c =>
        {
            c.Scan(scanner =>
            {
                scanner.AssemblyContainingType<Plugin>();
                // scanner.WithDefaultConventions();

                // scanner.AddAllTypesOf<PatchProcessor>();
                // scanner.AddAllTypesOf<IMainMenuTab>();
                scanner.AddAllTypesOf<EngineMethodClass>();
                scanner.ExcludeType<Movie.EngineMethods.Implementations.Env>();
            });

            // c.ForSingletonOf<PluginWrapper>().Use<PluginWrapper>();

            // c.ForSingletonOf<MonoBehEventInvoker>().Use<MonoBehEventInvoker>();
            // c.For<IMonoBehEventInvoker>().Use(x => x.GetInstance<MonoBehEventInvoker>());
            // c.For<IUpdateEvents>().Use(x => x.GetInstance<MonoBehEventInvoker>());
            //
            c.For<IStaticFieldManipulator>().Singleton().Use<FakeStaticFieldStorage>();

            c.ForSingletonOf<SyncFixedUpdate>().Use<SyncFixedUpdate>();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            // c.ForSingletonOf<SceneIndexNameTracker>().Use<SceneIndexNameTracker>();
            // c.For<ISceneIndexName>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            // c.For<IPluginInitialLoad>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            // c.For<IOnUpdate>().Use(x => x.GetInstance<SceneIndexNameTracker>());
            //
            // c.For<IGameInfo>().Singleton().Use<GameInfo.GameInfo>();
            //
            c.ForSingletonOf<GameInitialRestart.GameInitialRestart>().Use<GameInitialRestart.GameInitialRestart>();
            c.Forward<GameInitialRestart.GameInitialRestart, IGameInitialRestart>();
            c.Forward<GameInitialRestart.GameInitialRestart, IOnAwake>();
            c.Forward<GameInitialRestart.GameInitialRestart, IOnEnable>();
            c.Forward<GameInitialRestart.GameInitialRestart, IOnStart>();
            c.Forward<GameInitialRestart.GameInitialRestart, IOnFixedUpdate>();

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController.MonoBehaviourController>();

            c.For<ISceneWrapper>().Singleton().Use<SceneManagerWrapper>();

            c.For<IUnityWrapper>().Singleton().Use<UnityWrapper>();

            c.For<IRandomWrapper>().Singleton().Use<RandomWrapper>();

            c.For<ITimeWrapper>().Singleton().Use<TimeWrapper>();

            c.For<ILogger>().Singleton().Use<FakeLogger>();

            c.ForSingletonOf<DummyLogger>().Use<DummyLogger>();
            c.For<IMovieLogger>().Use(x => x.GetInstance<DummyLogger>());

            // before FileSystemManager
            c.ForSingletonOf<PatchReverseInvoker>().Use<PatchReverseInvoker>();
            c.For<IPatchReverseInvoker>().Use(x => x.GetInstance<PatchReverseInvoker>());

            // before VirtualEnvironment
            // c.ForSingletonOf<GameTime>().Use<GameTime>();
            // c.For<IOnPreUpdates>().Use(x => x.GetInstance<GameTime>());
            // c.For<IOnGameRestartResume>().Use(x => x.GetInstance<GameTime>());
            // c.For<IOnStart>().Use(x => x.GetInstance<GameTime>());
            //
            // // before VirtualEnvironment
            // c.ForSingletonOf<VirtualEnvironmentApplier>().Use<VirtualEnvironmentApplier>();
            // c.For<IOnPreUpdates>().Use(x => x.GetInstance<VirtualEnvironmentApplier>());
            //
            // // after VirtualEnvironmentApplier
            // c.ForSingletonOf<InputState>().Use<InputState>();
            // c.For<IOnGameRestart>().Use(x => x.GetInstance<InputState>());
            // c.For<IOnPreUpdates>().Use(x => x.GetInstance<InputState>());

            c.ForSingletonOf<VirtualEnvironment>().Use<VirtualEnvironment>();
            c.For<IOnGameRestartResume>().Use(x => x.GetInstance<VirtualEnvironment>());

            // after VirtualEnvironment
            // c.For<IOnGameRestartResume>().Use<UnityRngRestartInit>();

            c.ForSingletonOf<Movie.MovieRunner>().Use<Movie.MovieRunner>();
            c.Forward<Movie.MovieRunner, IMovieRunner>();
            c.Forward<Movie.MovieRunner, IOnPreUpdates>();

            c.ForSingletonOf<GameRestart.GameRestart>().Use<GameRestart.GameRestart>();
            c.Forward<GameRestart.GameRestart, IGameRestart>();
            c.Forward<GameRestart.GameRestart, IOnEnable>();
            c.Forward<GameRestart.GameRestart, IOnStart>();
            c.Forward<GameRestart.GameRestart, IOnFixedUpdate>();
            c.Forward<GameRestart.GameRestart, IOnAwake>();

            // c.ForSingletonOf<FileSystemManager>().Use<FileSystemManager>();
            // c.For<IOnGameRestart>().Use(x => x.GetInstance<FileSystemManager>());
            // c.For<IFileSystemManager>().Use(x => x.GetInstance<FileSystemManager>());
            //
            // c.ForSingletonOf<AsyncOperationTracker>().Use<AsyncOperationTracker>();
            // c.For<ISceneLoadTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            // c.For<IAssetBundleCreateRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            // c.For<IAssetBundleRequestTracker>().Use(x => x.GetInstance<AsyncOperationTracker>());
            // c.For<IOnLastUpdate>().Use(x => x.GetInstance<AsyncOperationTracker>());
            //
            // c.ForSingletonOf<SceneTracker>().Use<SceneTracker>();
            // c.For<ISceneTracker>().Use(x => x.GetInstance<SceneTracker>());
            // c.For<ILoadedSceneInfo>().Use(x => x.GetInstance<SceneTracker>());

            c.For<IUnityInstanceWrapFactory>().Singleton().Use<UnityInstanceWrapFactory>();

            // c.ForSingletonOf<MainThreadSpeedControl>().Use<MainThreadSpeedControl>();
            // c.For<IMainThreadSpeedControl>().Use(x => x.GetInstance<MainThreadSpeedControl>());
            // c.For<IOnUpdate>().Use(x => x.GetInstance<MainThreadSpeedControl>());
            //
            // c.ForSingletonOf<GameSpeedUnlocker.GameSpeedUnlocker>().Use<GameSpeedUnlocker.GameSpeedUnlocker>();
            // c.For<IGameSpeedUnlocker>().Use(x => x.GetInstance<GameSpeedUnlocker.GameSpeedUnlocker>());
            // c.For<IOnMovieStart>().Use(x => x.GetInstance<GameSpeedUnlocker.GameSpeedUnlocker>());
            // c.For<IOnMovieEnd>().Use(x => x.GetInstance<GameSpeedUnlocker.GameSpeedUnlocker>());

            c.For<IMovieParser>().Use<MovieParser>();

            c.For<IEngineMethodClassesFactory>().Use<EngineMethodClassesFactory>();

            c.ForSingletonOf<Env>().Use<Env>();
            c.Forward<Env, EngineMethodClass>();
            c.Forward<Env, IOnLastUpdate>();

            c.For<IMovieEngine>().Use<MovieEngine>();
            
            c.For<IFfmpegRunner>().Use<FfmpegRunner>();
        });

        return kernel;
    }
}