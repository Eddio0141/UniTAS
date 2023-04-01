using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Tests.Kernel;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public static class KernelUtils
{
    [Singleton(IncludeDifferentAssembly = true)]
    public class TimeWrapper : ITimeWrapper
    {
        public double CaptureFrameTime { get; set; }
        public bool IntFPSOnly => true;
    }

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

    [Singleton(IncludeDifferentAssembly = true)]
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

    [Singleton(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public class FakeLogger : ILogger
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

    [Singleton(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    public class FakeStaticFieldStorage : IStaticFieldManipulator
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
        });

        return kernel;
    }
}