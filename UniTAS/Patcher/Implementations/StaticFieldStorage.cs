using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.TrackInfo;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(timing: RegisterTiming.Entry)]
[ExcludeRegisterIfTesting]
public class StaticFieldStorage(
    ILogger logger,
    IClassStaticInfoTracker classStaticInfoTracker,
    ITryFreeMalloc freeMalloc)
    : IStaticFieldManipulator
{
    private readonly FieldInfo[] _ignoredFields =
    [
        // don't reset this, all hell breaks loose
        AccessTools.DeclaredField(AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager"),
            "s_AllowLoadScene")
    ];

    public void ResetStaticFields()
    {
        logger.LogDebug("resetting static fields");

        UnityEngine.Resources.UnloadUnusedAssets();

        foreach (var field in classStaticInfoTracker.StaticFields)
        {
            var typeName = field.DeclaringType?.FullName ?? "unknown_type";

            if (_ignoredFields.Contains(field))
            {
                logger.LogDebug($"ignoring static field: {typeName}.{field.Name}");
                continue;
            }

            logger.LogDebug($"resetting static field: {typeName}.{field.Name}");

            // only dispose if disposable in is in system namespace
            if (field.GetValue(null) is IDisposable disposable &&
                field.FieldType.Namespace?.StartsWith("System") is true)
            {
                logger.LogDebug("disposing object via IDisposable");
                disposable.Dispose();
            }

            freeMalloc?.TryFree(null, field);
            field.SetValue(null, null);
        }

        // idk if this even works
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var count = classStaticInfoTracker.StaticCtorInvokeOrder.Count;
        logger.LogDebug($"calling {count} static constructors");
        for (var i = 0; i < count; i++)
        {
            var staticCtorType = classStaticInfoTracker.StaticCtorInvokeOrder[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            logger.LogDebug($"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            try
            {
                cctor.Invoke(null, default);
            }
            catch (Exception e)
            {
                logger.LogDebug($"Exception thrown while calling static constructor: {e}");
            }

            // ik calling static ctors in the first place is illegal as fk but why does this prevent crashing??????
            Thread.Sleep(1);
        }
    }
}