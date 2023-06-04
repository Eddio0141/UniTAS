using HarmonyLib;
using StructureMap;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.DependencyInjection;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.DependencyInjection;

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
            var attributes = type.GetCustomAttributes(typeof(ForceInstantiateAttribute), false);
            if (attributes.Length == 0) continue;

            attributes = type.GetCustomAttributes(typeof(ExcludeRegisterIfTestingAttribute), false);
            if (attributes.Length > 0 && _isTesting) continue;

            _logger.LogDebug($"Force instantiating {type.Name}");
            _container.GetInstance(type);
        }
    }
}