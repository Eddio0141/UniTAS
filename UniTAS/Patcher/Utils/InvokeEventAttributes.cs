using HarmonyLib;
using UniTAS.Patcher.Services.Invoker;

namespace UniTAS.Patcher.Utils;

public static class InvokeEventAttributes
{
    /// <summary>
    /// Invokes all methods with TAttribute
    /// </summary>
    public static void Invoke<TAttribute>()
        where TAttribute : InvokerAttribute, new()
    {
        var assembly = typeof(TAttribute).Assembly;
        var types = AccessTools.GetTypesFromAssembly(assembly);

        var invokerAttribute = new TAttribute();
        foreach (var type in types)
        {
            invokerAttribute.HandleType(type);
        }
    }
}