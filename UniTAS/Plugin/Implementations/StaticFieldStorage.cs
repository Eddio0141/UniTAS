using System.Diagnostics;
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
        _logger.LogDebug("calling static constructors");
        var staticCtorInvokeOrderListCount = Tracker.StaticCtorInvokeOrderList.Count;
        for (var i = 0; i < staticCtorInvokeOrderListCount; i++)
        {
            var staticCtorType = Tracker.StaticCtorInvokeOrderList[i];
            var cctor = staticCtorType.TypeInitializer;
            if (cctor == null) continue;
            Trace.Write($"Calling static constructor for type: {cctor.DeclaringType?.FullName ?? "unknown_type"}");
            cctor.Invoke(null, default);
        }
    }
}