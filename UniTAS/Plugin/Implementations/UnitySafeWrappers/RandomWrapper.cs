using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class RandomWrapper : IRandomWrapper
{
    private readonly ILogger _logger;

    private readonly MethodBase _initState =
        typeof(Random).GetMethod("InitState", AccessTools.all, null, new[] { typeof(int) }, null);

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
                _initState.Invoke(null, new object[] { value });
                return;
            }

            Random.seed = value;
        }
    }
}