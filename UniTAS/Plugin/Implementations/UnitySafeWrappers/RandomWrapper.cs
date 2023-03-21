using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class RandomWrapper : IRandomWrapper
{
    private readonly MethodBase _initState =
        typeof(Random).GetMethod("InitState", AccessTools.all, null, new[] { typeof(int) }, null);

    public int Seed
    {
        set
        {
            Trace.Write($"Setting unity random seed to {value}");

            if (_initState != null)
            {
                _initState.Invoke(null, new object[] { value });
                return;
            }

            Random.seed = value;
        }
    }
}