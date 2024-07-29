using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations;

[Register]
[ForceInstantiate]
public class UnityRuntimeInitAttributeInvoker
{
    private readonly MethodBase[] _beforeSceneLoad;

    // after OnEnable. hook on Start and FixedUpdate. FixedUpdate is a guarantee to be invoked so that's the fail-safe.
    private readonly MethodBase[] _beforeStart;

    private readonly IGameRestart _gameRestart;
    private readonly IUpdateEvents _updateEvents;
    private readonly ILogger _logger;

    private bool _invokedBeforeSceneLoad;
    private bool _invokedBeforeStart;

    public UnityRuntimeInitAttributeInvoker(IGameRestart gameRestart, IUpdateEvents updateEvents, ILogger logger)
    {
        var runtimeInitializeOnLoadMethodAttribute =
            AccessTools.TypeByName("UnityEngine.RuntimeInitializeOnLoadMethodAttribute");
        if (runtimeInitializeOnLoadMethodAttribute == null) return;
        var runtimeInitializeLoadType = AccessTools.TypeByName("UnityEngine.RuntimeInitializeLoadType");

        _gameRestart = gameRestart;
        _updateEvents = updateEvents;
        _logger = logger;
        _gameRestart.OnGameRestart += GameRestart;
        _gameRestart.OnGameRestart += (_, _) =>
        {
            _invokedBeforeSceneLoad = false;
            _invokedBeforeStart = false;
        };

        var loadType = AccessTools.PropertyGetter(runtimeInitializeOnLoadMethodAttribute, "loadType");

        var dlls = Directory.GetFiles(Paths.ManagedPath, "*.dll").Select(Path.GetFileNameWithoutExtension).ToList();
        var assemblies = AccessTools.AllAssemblies().Where(x => dlls.Contains(x.GetName().Name))
            .SelectMany(AccessTools.GetTypesFromAssembly).ToArray();

        _beforeSceneLoad = GetMethodsByAttribute(assemblies,
            ["SubsystemRegistration", "AfterAssembliesLoaded", "BeforeSplashScreen", "BeforeSceneLoad"],
            loadType,
            runtimeInitializeOnLoadMethodAttribute, runtimeInitializeLoadType).ToArray();

        _beforeStart = GetMethodsByAttribute(assemblies,
            ["AfterSceneLoad"],
            loadType,
            runtimeInitializeOnLoadMethodAttribute, runtimeInitializeLoadType).ToArray();
    }

    private static IEnumerable<MethodBase> GetMethodsByAttribute(Type[] types, string[] loadTypes, MethodBase loadType,
        Type runtimeInitializeOnLoadMethodAttribute, Type runtimeInitializeLoadType)
    {
        var loadTypesFiltered = loadTypes
            .Where(x => Enum.IsDefined(runtimeInitializeLoadType, x))
            .Select(x => Enum.Parse(runtimeInitializeLoadType, x)).ToList();
        var methodWithAttributes = types.SelectMany(x => x
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0))
            .Where(x => x.GetCustomAttributes(runtimeInitializeOnLoadMethodAttribute, true).Length > 0);

        var methodsUnordered = new List<(MethodBase, int)>();

        foreach (var methodWithAttribute in methodWithAttributes)
        {
            var attrs = methodWithAttribute.GetCustomAttributes(runtimeInitializeOnLoadMethodAttribute, true)
                    .Select(x => loadType.Invoke(x, null));

            foreach (var attr in attrs)
            {
                var loadTypeIndex = loadTypesFiltered.IndexOf(attr);
                if (loadTypeIndex < 0) continue;

                methodsUnordered.Add((methodWithAttribute, loadTypeIndex));
                continue;
            }
        }

        return methodsUnordered.OrderBy(x => x.Item2).Select(x => x.Item1);
    }

    // only invoke after game restart
    private void GameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        _updateEvents.OnAwakeActual += InvokeBeforeSceneLoad;

        _updateEvents.OnStartActual += InvokeBeforeStart;
        _updateEvents.OnFixedUpdateActual += InvokeBeforeStart;

        _gameRestart.OnGameRestart -= GameRestart;
    }

    private void InvokeBeforeSceneLoad()
    {
        if (_invokedBeforeSceneLoad) return;
        _invokedBeforeSceneLoad = true;

        _logger.LogDebug($"Invoking BeforeSceneLoad methods, callback count: {_beforeSceneLoad.Length}");

        foreach (var method in _beforeSceneLoad)
        {
            _logger.LogDebug($"Invoking BeforeSceneLoad method {method.DeclaringType?.Name}.{method.Name}");
            method.Invoke(null, null);
        }
    }

    private void InvokeBeforeStart()
    {
        if (_invokedBeforeStart) return;
        _invokedBeforeStart = true;

        _logger.LogDebug($"Invoking BeforeStart methods, callback count: {_beforeStart.Length}");

        foreach (var method in _beforeStart)
        {
            _logger.LogDebug($"Invoking BeforeStart method {method.DeclaringType?.Name}.{method.Name}");
            method.Invoke(null, null);
        }
    }
}
