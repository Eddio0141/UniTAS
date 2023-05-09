using System;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class StaticFieldStorage : IStaticFieldManipulator
{
    private readonly ILogger _logger;

    public StaticFieldStorage(ILogger logger)
    {
        _logger = logger;
    }

    public void ResetStaticFields()
    {
        _logger.LogDebug("resetting static fields");

        UnityEngine.Resources.UnloadUnusedAssets();

        var staticFieldStorage = Tracker.StaticFields;
        const int fieldResetCount = 100;
        var fieldReset = fieldResetCount;

        foreach (var field in staticFieldStorage)
        {
            var typeName = field.DeclaringType?.FullName ?? "unknown_type";
            _logger.LogDebug($"resetting static field: {typeName}.{field.Name}");
            field.SetValue(null, null);
            fieldReset--;

            if (fieldReset != 0) continue;
            fieldReset = fieldResetCount;

            // just in case
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // lol
        GC.Collect();
        GC.WaitForPendingFinalizers();

        _logger.LogDebug("calling static constructors");
        var staticCtorInvokeOrderListCount = Tracker.StaticCtorInvokeOrder.Count;
        for (var i = 0; i < staticCtorInvokeOrderListCount; i++)
        {
            var staticCtorType = Tracker.StaticCtorInvokeOrder[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            _logger.LogDebug($"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            cctor.Invoke(null, default);
        }
    }
}