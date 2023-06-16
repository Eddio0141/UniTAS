using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Configuration;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Patcher;
using UniTAS.Patcher.Implementations.DependencyInjection;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace Patcher.Tests;

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
    public class DummyMouseEnvLegacySystem : IMouseStateEnvLegacySystem
    {
        public bool AnyButtonDown { get; }
        public bool AnyButtonHeld { get; }
        public bool MousePresent { get; }
        public Vector2 Position { get; set; }
        public Vector2 Scroll { get; set; }

        public bool IsButtonHeld(MouseButton button)
        {
            return false;
        }

        public bool IsButtonDown(MouseButton button)
        {
            return false;
        }

        public bool IsButtonUp(MouseButton button)
        {
            return false;
        }

        public void HoldButton(MouseButton button)
        {
        }

        public void ReleaseButton(MouseButton button)
        {
        }
    }

    [Register(IncludeDifferentAssembly = true)]
    public class ConfigDummy : IConfig
    {
        public ConfigFile ConfigFile => null!;
    }

    [Register(IncludeDifferentAssembly = true)]
    public class SceneManagerWrapperDummy : ISceneWrapper
    {
        public void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
            LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame)
        {
        }

        public void LoadScene(int buildIndex)
        {
        }

        public int TotalSceneCount => 0;
        public int ActiveSceneIndex => 0;
        public string ActiveSceneName => "";
    }

    [Register(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    public class TimeEnvDummy : ITimeEnv
    {
        public double FrameTime { get; set; }
        public DateTime CurrentTime { get; }
        public ulong RenderedFrameCountOffset { get; }
        public ulong FrameCountRestartOffset { get; }
        public double SecondsSinceStartUp { get; }
        public double UnscaledTime { get; }
        public double FixedUnscaledTime { get; }
        public double ScaledTime { get; }
        public double ScaledFixedTime { get; }
        public double RealtimeSinceStartup { get; }
    }

    [Register(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    private class DrawingDummy : IDrawing
    {
        public void FillBox(int x, int y, int width, int height, Color32 color)
        {
        }

        public void PrintText(int x, int y, string text)
        {
        }
    }

    public static Container Init()
    {
        var kernel = new Container(c =>
        {
            c.ForSingletonOf<DiscoverAndRegister>().Use<DiscoverAndRegister>();
            c.For<IDiscoverAndRegister>().Use(x => x.GetInstance<DiscoverAndRegister>());

            c.ForSingletonOf<FakeLogger>().Use<FakeLogger>();
            c.For<ILogger>().Use(x => x.GetInstance<FakeLogger>());
        });

        kernel.Configure(c =>
        {
            kernel.GetInstance<IDiscoverAndRegister>().Register<FakeStaticFieldStorage>(c);
            kernel.GetInstance<IDiscoverAndRegister>().Register<InfoPrintAndWelcome>(c);
        });

        var forceInstantiateTypes = kernel.GetInstance<IForceInstantiateTypes>();
        forceInstantiateTypes.InstantiateTypes<InfoPrintAndWelcome>();
        forceInstantiateTypes.InstantiateTypes<DummyMouseEnvLegacySystem>();

        return kernel;
    }
}