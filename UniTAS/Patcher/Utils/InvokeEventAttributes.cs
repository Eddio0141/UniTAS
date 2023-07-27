using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MonoMod.Utils;
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
        var toBeInvoked = new List<TupleValue<MethodBase, InvokerAttribute>>();

        foreach (var type in types)
        {
            // check type cctor invoke
            // if (type.GetCustomAttributes(typeof(TAttribute), false).Length > 0)
            // {
            //     // invoke type cctor
            //     StaticLogger.Log.LogDebug(
            //         $"Invoking type cctor for {type.FullName} with attribute {typeof(TAttribute).FullName}");
            //     RuntimeHelpers.RunClassConstructor(type.TypeHandle);
            // }

            // check method invoke
            var methods = type.GetMethods(AccessTools.all);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(TAttribute), false);
                if (attributes.Length == 0)
                    continue;

                var invokerAttribute = (InvokerAttribute)attributes.First(x => x is InvokerAttribute);
                toBeInvoked.Add(new(method, invokerAttribute));
            }
        }

        // sort it
        toBeInvoked = toBeInvoked.OrderBy(x => (int)x.Item2.Priority).ToList();

        // actually invoke it
        foreach (var invokeInfo in toBeInvoked)
        {
            var method = invokeInfo.Item1;
            StaticLogger.Log.LogDebug(
                $"Invoking method {method.Name} for {method.GetRealDeclaringType().FullName} with attribute {typeof(TAttribute).FullName} and priority {(int)invokeInfo.Item2.Priority}");
            method.Invoke(null, null);
        }
    }
}