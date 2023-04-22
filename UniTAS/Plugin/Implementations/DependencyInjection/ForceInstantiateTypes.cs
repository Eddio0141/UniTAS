using HarmonyLib;
using StructureMap;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.DependencyInjection;

namespace UniTAS.Plugin.Implementations.DependencyInjection;

[Register]
public class ForceInstantiateTypes : IForceInstantiateTypes
{
    private readonly IContainer _container;

    public ForceInstantiateTypes(IContainer container)
    {
        _container = container;
    }

    public void InstantiateTypes()
    {
        var allTypes = AccessTools.GetTypesFromAssembly(typeof(ForceInstantiateTypes).Assembly);

        foreach (var type in allTypes)
        {
            var attributes = type.GetCustomAttributes(typeof(ForceInstantiateAttribute), true);
            if (attributes.Length == 0) continue;

            _container.GetInstance(type);
        }
    }
}