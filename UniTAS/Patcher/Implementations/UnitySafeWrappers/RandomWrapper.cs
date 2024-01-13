using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class RandomWrapper : IRandomWrapper
{
    private readonly ILogger _logger;

    private readonly MethodBase _initState =
        typeof(Random).GetMethod("InitState", AccessTools.all, null, [typeof(int)], null);

    public RandomWrapper(ILogger logger)
    {
        _logger = logger;
    }

    public int Seed
    {
        set
        {
            _logger.LogDebug($"Setting unity random seed to {value}");

            if (_initState != null)
            {
                _initState.Invoke(null, [value]);
                return;
            }

            Random.seed = value;
        }
    }
}