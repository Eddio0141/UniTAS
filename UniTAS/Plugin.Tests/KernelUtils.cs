using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Plugin.Implementations.DependencyInjection;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Models.DependencyInjection;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.DependencyInjection;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class KernelUtils
{
    [Singleton(IncludeDifferentAssembly = true)]
    public class TimeWrapper : ITimeWrapper
    {
        public double CaptureFrameTime { get; set; }
        public bool IntFPSOnly => true;
    }

    [Singleton(IncludeDifferentAssembly = true)]
    public class Env : EngineMethodClass, IOnLastUpdateUnconditional
    {
        public float Fps { get; set; }
        public float Frametime { get; set; }

        [MoonSharpHidden]
        public void OnLastUpdateUnconditional()
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

    [Singleton(RegisterPriority.FirstUpdateSkipOnRestart, IncludeDifferentAssembly = true)]
    public class TestPriority : IOnPreUpdatesActual
    {
        public void PreUpdateActual()
        {
        }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    public class TestPriority2 : IOnPreUpdatesActual
    {
        public void PreUpdateActual()
        {
        }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public class DummyMouseEnv : IMouseStateEnv
    {
        public bool MousePresent { get; }
        public Vector2 Position { get; set; }
        public bool LeftClick { get; set; }
        public bool LeftClickDown { get; }
        public bool LeftClickUp { get; }
        public bool RightClick { get; set; }
        public bool RightClickDown { get; }
        public bool RightClickUp { get; }
        public bool MiddleClick { get; set; }
        public bool MiddleClickDown { get; }
        public bool MiddleClickUp { get; }
        public Vector2 Scroll { get; set; }
    }

    public static Container Init()
    {
        var kernel = new Container(c =>
        {
            c.ForSingletonOf<DiscoverAndRegister>().Use<DiscoverAndRegister>();
            c.For<IDiscoverAndRegister>().Use(x => x.GetInstance<DiscoverAndRegister>());

            c.ForSingletonOf<ConfigFile>().Use(new ConfigFile("test", false));
        });

        kernel.Configure(c =>
        {
            kernel.GetInstance<IDiscoverAndRegister>().Register<FakeStaticFieldStorage>(c);
            kernel.GetInstance<IDiscoverAndRegister>().Register<PluginWrapper>(c);
        });

        var forceInstantiateTypes = kernel.GetInstance<IForceInstantiateTypes>();
        forceInstantiateTypes.InstantiateTypes<DummyMouseEnv>();

        return kernel;
    }
}