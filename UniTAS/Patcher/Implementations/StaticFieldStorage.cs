using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.TrackInfo;

namespace UniTAS.Patcher.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton(timing: RegisterTiming.Entry)]
[ExcludeRegisterIfTesting]
public class StaticFieldStorage : IStaticFieldManipulator
{
    private readonly ILogger _logger;
    private readonly IClassStaticInfoTracker _classStaticInfoTracker;

    public StaticFieldStorage(ILogger logger, IClassStaticInfoTracker classStaticInfoTracker)
    {
        _logger = logger;
        _classStaticInfoTracker = classStaticInfoTracker;
    }

    public void ResetStaticFields()
    {
        _logger.LogDebug("resetting static fields");

        UnityEngine.Resources.UnloadUnusedAssets();

        foreach (var field in _classStaticInfoTracker.StaticFields)
        {
            var typeName = field.DeclaringType?.FullName ?? "unknown_type";
            _logger.LogDebug($"resetting static field: {typeName}.{field.Name}");

            // only dispose if disposable in is in system namespace
            if (field.GetValue(null) is IDisposable disposable &&
                field.FieldType.Namespace?.StartsWith("System") is true)
            {
                _logger.LogDebug("disposing object via IDisposable");
                disposable.Dispose();
            }

            field.SetValue(null, null);
        }

        // idk if this even works
        GC.Collect();
        GC.WaitForPendingFinalizers();

        _logger.LogDebug("calling static constructors");
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < _classStaticInfoTracker.StaticCtorInvokeOrder.Count; i++)
        {
            var staticCtorType = _classStaticInfoTracker.StaticCtorInvokeOrder[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            _logger.LogDebug($"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            try
            {
                cctor.Invoke(null, default);
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Exception thrown while calling static constructor: {e}");
            }
        }
    }
}