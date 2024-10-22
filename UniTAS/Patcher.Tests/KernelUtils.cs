using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AssetsTools.NET.Extra;
using BepInEx.Configuration;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Patcher.ContainerBindings.UnityEvents;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.Implementations.DependencyInjection;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.Unity.Collections;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Interfaces.UnitySafeWrappers;
using UniTAS.Patcher.Models.Customization;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.GlobalHotkeyListener;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Customization;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers;
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

    [Singleton(IncludeDifferentAssembly = true)]
    public class TestPriority : IOnPreUpdateActual
    {
        public void PreUpdateActual()
        {
        }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    public class TestPriority2 : IOnPreUpdateActual
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
        public ConfigFile BepInExConfigFile => null!;

        public bool TryGetBackendEntry<T>(string key, out T value)
        {
            throw new NotImplementedException();
        }

        public void WriteBackendEntry<T>(string key, T value)
        {
            throw new NotImplementedException();
        }
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

        public void LoadScene(string name)
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
        public double TimeTolerance { get; }
    }

    [Register(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    private class BindsDummy : IBinds
    {
        public Bind Create(BindConfig config, bool noGenConfig)
        {
            return null!;
        }

        public Bind Get(string name)
        {
            return null!;
        }

        public ReadOnlyCollection<Bind> AllBinds => null!;
    }

    [Singleton(IncludeDifferentAssembly = true)]
    public class SyncFixedUpdateCycleDummy : ISyncFixedUpdateCycle
    {
        private readonly Queue<Action> _callbacks = new();

        public void OnSync(Action callback, double invokeOffset = 0, uint fixedUpdateIndex = 0)
        {
            _callbacks.Enqueue(callback);
        }

        public void ForceLastCallback()
        {
            _callbacks.Dequeue().Invoke();
        }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    private class GlobalHotkeyListenerDummy : IGlobalHotkey
    {
        public void AddGlobalHotkey(GlobalHotkey config)
        {
        }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    private class UpdateInvokeOffsetDummy : IUpdateInvokeOffset
    {
        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public double Offset { get; }
    }

    [Singleton(IncludeDifferentAssembly = true)]
    private class AssetsManagerDummy : IAssetsManager
    {
        public AssetsManager Instance => null!;
    }

    [Singleton(IncludeDifferentAssembly = true)]
    private class WindowEnvDummy : IWindowEnv
    {
        public IResolutionWrapper CurrentResolution { get; set; } = null!;
        public bool FullScreen { get; set; }
        public IResolutionWrapper[] ExtraSupportedResolutions { get; set; } = null!;
        public FullScreenModeWrap FullScreenMode { get; set; } = null!;
    }

    [Singleton(IncludeDifferentAssembly = true)]
    public class UnityInstanceWrapFactoryDummy(IContainer container) : IUnityInstanceWrapFactory
    {
        public T Create<T>(object instance) where T : class
        {
            return container.With(instance).GetInstance<T>();
        }

        public T CreateNew<T>(params object[] args) where T : class
        {
            // TODO: this shit is stupid i hate this, what do i even do
            if (typeof(T) == typeof(LoadSceneParametersWrapper))
            {
                return (new LoadSceneParametersWrapper(null) as T)!;
            }

            if (typeof(T) == typeof(SceneWrapper))
            {
                return (new SceneWrapper(null) as T)!;
            }

            if (typeof(T) == typeof(RefreshRateWrap))
            {
                var newRr = new RefreshRateWrap(null);

                if (args.Length == 0)
                {
                    return (newRr as T)!;
                }

                if (args.Length == 1)
                {
                    if (args[0] is double d)
                    {
                        newRr.Rate = d;
                        return (newRr as T)!;
                    }

                    throw new ArgumentException();
                }

                newRr.Denominator = (uint)args[1];
                newRr.Numerator = (uint)args[0];
                return (newRr as T)!;
            }

            if (typeof(T) == typeof(IResolutionWrapper))
            {
                if (args.Length == 0)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.
                    return new ResolutionWrapperDummy(null) as T;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }

                return (new ResolutionWrapperDummy((int)args[0], (int)args[1], (RefreshRateWrap)args[2]) as T)!;
            }

            // NativeArrayWrapper<T>, I don't care whatever, fix when more generics shit
            var tGenerics = typeof(T).GetGenericArguments();
            var newNativeArray = typeof(NativeArrayWrapper<>).MakeGenericType(tGenerics);
            return (Activator.CreateInstance(newNativeArray, args) as T)!;
        }
    }

    private class ResolutionWrapperDummy(object instance) : UnityInstanceWrap(instance), IResolutionWrapper
    {
        protected override Type WrappedType => null!;

        public int Height { get; set; }
        public int Width { get; set; }

        public ResolutionWrapperDummy(int width, int height, RefreshRateWrap refreshRateWrap) : this(null!)
        {
            Width = width;
            Height = height;
            RefreshRateWrap = refreshRateWrap;
        }

        public RefreshRateWrap RefreshRateWrap { get; set; } = null!;
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

        var timings = new[]
        {
            RegisterTiming.Entry,
            RegisterTiming.UnityInit
        };

        Assert.Equal(Enum.GetValues(typeof(RegisterTiming)).Length, timings.Length);

        foreach (var timing in timings)
        {
            kernel.Configure(c =>
            {
                kernel.GetInstance<IDiscoverAndRegister>().Register<InfoPrintAndWelcome>(c, timing);
                kernel.GetInstance<IDiscoverAndRegister>().Register<FakeStaticFieldStorage>(c, timing);
            });

            var forceInstantiateTypes = kernel.GetInstance<IForceInstantiateTypes>();
            forceInstantiateTypes.InstantiateTypes<InfoPrintAndWelcome>(timing);
            forceInstantiateTypes.InstantiateTypes<DummyMouseEnvLegacySystem>(timing);
        }

        UnityEventInvokers.Init(kernel);

        return kernel;
    }
}