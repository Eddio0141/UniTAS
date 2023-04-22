using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.DependencyInjection;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations.DependencyInjection;

[Register]
public class ForceInstantiateTypes : IForceInstantiateTypes
{
    private readonly IContainer _container;
    private readonly ILogger _logger;

    private readonly bool _isTesting = AccessTools.TypeByName("Xunit.FactAttribute") != null;

    public ForceInstantiateTypes(IContainer container, ILogger logger)
    {
        _container = container;
        _logger = logger;
    }

    public void InstantiateTypes<TAssemblyContainingType>()
    {
        var allTypes = AccessTools.GetTypesFromAssembly(typeof(TAssemblyContainingType).Assembly);

        foreach (var type in allTypes)
        {
            var attributes = type.GetCustomAttributes(typeof(ForceInstantiateAttribute), true);
            if (attributes.Length == 0) continue;

            attributes = type.GetCustomAttributes(typeof(ExcludeRegisterIfTestingAttribute), true);
            if (attributes.Length > 0 && _isTesting) continue;

            _logger.LogDebug($"Force instantiating {type}");
            _container.GetInstance(type);
        }
    }
}