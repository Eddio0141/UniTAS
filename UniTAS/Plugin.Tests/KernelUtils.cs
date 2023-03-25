using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Plugin.Implementations;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Implementations.GameRestart;
using UniTAS.Plugin.Implementations.Movie.Engine;
using UniTAS.Plugin.Implementations.Movie.Parser;
using UniTAS.Plugin.Implementations.UnitySafeWrappers;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.UnitySafeWrappers;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Tests.Kernel;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public static class KernelUtils
{
    [Singleton(IncludeDifferentAssembly = true)]
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

        public void LogError(object data, bool whenPlayingMovie = false)
        {
            Errors.Add(data.ToString() ?? string.Empty);
        }

        public void LogInfo(object data, bool whenPlayingMovie = false)
        {
            Infos.Add(data.ToString() ?? string.Empty);
        }

        public void LogWarning(object data, bool whenPlayingMovie = false)
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
                scanner.AssemblyContainingType<KernelTests>();
                scanner.Convention<DependencyInjectionConvention>();
            });

            c.ForSingletonOf<ConfigFile>().Use(new ConfigFile("test", false));

            c.For<IStaticFieldManipulator>().Singleton().Use<FakeStaticFieldStorage>();

            c.ForSingletonOf<SyncFixedUpdate>().Use<SyncFixedUpdate>();
            c.For<ISyncFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnFixedUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());
            c.For<IOnUpdate>().Use(x => x.GetInstance<SyncFixedUpdate>());

            c.ForSingletonOf<GameInitialRestart>().Use<GameInitialRestart>();
            c.Forward<GameInitialRestart, IGameInitialRestart>();
            c.Forward<GameInitialRestart, IOnAwake>();
            c.Forward<GameInitialRestart, IOnEnable>();
            c.Forward<GameInitialRestart, IOnStart>();
            c.Forward<GameInitialRestart, IOnFixedUpdate>();

            c.For<IMonoBehaviourController>().Singleton().Use<MonoBehaviourController>();

            c.For<ISceneWrapper>().Singleton().Use<SceneManagerWrapper>();

            c.For<IRandomWrapper>().Singleton().Use<RandomWrapper>();

            c.For<ITimeWrapper>().Singleton().Use<TimeWrapper>();

            c.For<ILogger>().Singleton().Use<FakeLogger>();

            c.ForSingletonOf<DummyLogger>().Use<DummyLogger>();
            c.For<IMovieLogger>().Use(x => x.GetInstance<DummyLogger>());

            // before FileSystemManager
            c.ForSingletonOf<PatchReverseInvoker>().Use<PatchReverseInvoker>();
            c.For<IPatchReverseInvoker>().Use(x => x.GetInstance<PatchReverseInvoker>());

            c.ForSingletonOf<Implementations.Movie.MovieRunner>().Use<Implementations.Movie.MovieRunner>();
            c.Forward<Implementations.Movie.MovieRunner, IMovieRunner>();
            c.Forward<Implementations.Movie.MovieRunner, IOnPreUpdates>();

            c.ForSingletonOf<GameRestart>().Use<GameRestart>();
            c.Forward<GameRestart, IGameRestart>();
            c.Forward<GameRestart, IOnEnable>();
            c.Forward<GameRestart, IOnStart>();
            c.Forward<GameRestart, IOnFixedUpdate>();
            c.Forward<GameRestart, IOnAwake>();

            c.For<IUnityInstanceWrapFactory>().Singleton().Use<UnityInstanceWrapFactory>();

            c.For<IMovieParser>().Use<MovieParser>();

            c.For<IEngineModuleClassesFactory>().Use<EngineModuleClassesFactory>();

            c.For<IMovieEngine>().Use<MovieEngine>();
        });

        return kernel;
    }
}