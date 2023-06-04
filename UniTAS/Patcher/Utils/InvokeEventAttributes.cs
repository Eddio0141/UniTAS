using System.Runtime.CompilerServices;
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
        StaticLogger.Log.LogDebug($"Invoking all methods with attribute {typeof(TAttribute).FullName}");

        var assembly = typeof(TAttribute).Assembly;
        var types = AccessTools.GetTypesFromAssembly(assembly);

        foreach (var type in types)
        {
            // check type cctor invoke
            if (type.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
            {
                // invoke type cctor
                StaticLogger.Log.LogDebug(
                    $"Invoking type cctor for {type.FullName} with attribute {typeof(TAttribute).FullName}");
                RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            }

            // check method invoke
            var methods = type.GetMethods(AccessTools.all);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(TAttribute), false);
                if (attributes.Length == 0)
                    continue;

                StaticLogger.Log.LogDebug(
                    $"Invoking method {method.Name} for {type.FullName} with attribute {typeof(TAttribute).FullName}");
                method.Invoke(null, null);
            }
        }
    }
}