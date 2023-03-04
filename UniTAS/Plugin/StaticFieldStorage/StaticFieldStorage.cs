using System;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Runtime;
using UniTAS.Plugin.Logger;

namespace UniTAS.Plugin.StaticFieldStorage;

// ReSharper disable once ClassNeverInstantiated.Global
public class StaticFieldStorage : IStaticFieldManipulator
{
    private readonly ILogger _logger;

    public StaticFieldStorage(ILogger logger)
    {
        _logger = logger;
    }

    public void ResetStaticFields()
    {
        Tracker.StopStaticCtorExecution = true;
        ResetFields();
        Tracker.StopStaticCtorExecution = false;
        InvokeStaticCtors();
    }

    private void ResetFields()
    {
        _logger.LogDebug("setting static fields");
        var staticCtorInvokeOrderListCount = Tracker.StaticCtorInvokeOrderList.Count;
        for (var i = 0; i < staticCtorInvokeOrderListCount; i++)
        {
            var type = Tracker.StaticCtorInvokeOrderList[i];

            var fields = AccessTools.GetDeclaredFields(type).Where(x =>
                x.IsStatic && !x.IsLiteral).ToArray();

            foreach (var field in fields)
            {
                Trace.Write(
                    $"Setting static field: {field.DeclaringType?.FullName ?? "unknown_type"}.{field.Name} to null");
                field.SetValue(null, null);
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void InvokeStaticCtors()
    {
        _logger.LogDebug("calling static constructors");
        var staticCtorInvokeOrderListCount = Tracker.StaticCtorInvokeOrderList.Count;
        for (var i = 0; i < staticCtorInvokeOrderListCount; i++)
        {
            var staticCtorType = Tracker.StaticCtorInvokeOrderList[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            Trace.Write(
                $"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            cctor.Invoke(null, default);
        }
    }
}