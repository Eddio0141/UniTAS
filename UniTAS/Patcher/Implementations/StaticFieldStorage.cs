using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.TrackInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(RegisterPriority.StaticFieldStorage)]
[ExcludeRegisterIfTesting]
public class StaticFieldStorage(
    ILogger logger,
    IClassStaticInfoTracker classStaticInfoTracker,
    ITryFreeMalloc freeMalloc,
    IFinalizeSuppressor finalizeSuppressor) : IOnPreGameRestart
{
    public void OnPreGameRestart()
    {
        logger.LogDebug("resetting static fields");

        finalizeSuppressor.DisableFinalizeInvoke = true;

        // UnityEngine.Resources.UnloadUnusedAssets();

        var bench = Bench.Measure();

        var fieldsCount = classStaticInfoTracker.StaticFields.Count;
        var ctorInvokeCount = classStaticInfoTracker.StaticCtorInvokeOrder.Count;

        for (var i = 0; i < fieldsCount; i++)
        {
            var field = classStaticInfoTracker.StaticFields[i];
            var typeName = field.DeclaringType?.FullName ?? "unknown_type";

            logger.LogDebug($"resetting static field: {typeName}.{field.Name}");

            object fieldValue;
            try
            {
                fieldValue = field.GetValue(null);
            }
            catch (Exception e)
            {
                logger.LogWarning(
                    $"exception thrown during resetting static field, skipping `{typeName}.{field.Name}`: {e}");
                continue;
            }

            // only dispose if disposable in is in system namespace
            if (fieldValue is IDisposable disposable &&
                field.FieldType.Namespace?.StartsWith("System") is true)
            {
                logger.LogDebug("disposing object via IDisposable");
                disposable.Dispose();
            }

            freeMalloc?.TryFree(null, field);
            field.SetValue(null, null);
        }

        bench.Dispose();

        // idk if this even works
        GC.Collect();
        GC.WaitForPendingFinalizers();

        logger.LogDebug($"calling {ctorInvokeCount} static constructors");

        bench = Bench.Measure();
        for (var i = 0; i < ctorInvokeCount; i++)
        {
            var staticCtorType = classStaticInfoTracker.StaticCtorInvokeOrder[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            logger.LogDebug($"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");

            LoggingUtils.DiskLogger.Flush();
            try
            {
                cctor.Invoke(null, null);
            }
            catch (Exception e)
            {
                logger.LogDebug($"Exception thrown while calling static constructor: {e}");
            }
        }

        finalizeSuppressor.DisableFinalizeInvoke = false;

        bench.Dispose();
    }
}