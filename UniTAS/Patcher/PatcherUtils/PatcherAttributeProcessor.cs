using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;

namespace UniTAS.Patcher.PatcherUtils;

public class PatcherAttributeProcessor
{
    // find attribute of type PatcherAttribute
    private static readonly MethodInfo[] PatcherMethods = FilterPatcherMethods(Assembly.GetExecutingAssembly()
        .GetTypes()
        .SelectMany(t => t.GetMethods(AccessTools.all)));

    public static MethodInfo[] FilterPatcherMethods(IEnumerable<MethodInfo> methods)
    {
        return methods.Where(m => m.GetCustomAttributes(typeof(PatcherAttribute), false).Length > 0)
            // match signature of either `static void Method(AssemblyDefinition)` or `static void Method(ref AssemblyDefinition)`
            .Where(m => m.IsStatic && m.GetParameters().Length == 1 && m.ReturnType == typeof(void) &&
                        m.GetParameters()[0].ParameterType == typeof(AssemblyDefinition) ||
                        m.GetParameters()[0].ParameterType == typeof(AssemblyDefinition).MakeByRefType())
            .OrderByDescending(
                x => ((PatcherAttribute)x.GetCustomAttributes(typeof(PatcherAttribute), false)[0]).Priority)
            .ToArray();
    }

    public static void Patch(ref AssemblyDefinition assembly)
    {
        // patch each method
        foreach (var method in PatcherMethods)
        {
            method.Invoke(null, new object[] { assembly });
        }
    }
}