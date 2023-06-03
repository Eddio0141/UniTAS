using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UniTAS.Patcher.Services.Invoker;

namespace UniTAS.Patcher.Interfaces.Invoker;

/// <summary>
/// Invokes static constructor <see cref="Entry.Finish"/> invoke
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class InvokeOnPatcherFinishAttribute : InvokerAttribute
{
    public override void HandleType(Type type)
    {
        // check type cctor invoke
        if (type.GetCustomAttributes(typeof(InvokeOnPatcherFinishAttribute), false).Length > 0)
        {
            // invoke type cctor
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }

        // check method invoke
        var methods = type.GetMethods(AccessTools.all);

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(typeof(InvokeOnPatcherFinishAttribute), false);
            if (attributes.Length == 0)
                continue;

            method.Invoke(null, null);
        }
    }
}