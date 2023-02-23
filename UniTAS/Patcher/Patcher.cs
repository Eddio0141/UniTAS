using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Patcher
{
    // List of assemblies to patch
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static IEnumerable<string> TargetDLLs { get; } = new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

    private static bool _patchedHarmonyEarlyPatcher;

    // Patches the assemblies
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Patch(ref AssemblyDefinition assembly)
    {
        if (_patchedHarmonyEarlyPatcher) return;

        // find MonoBehaviour
        var monoBehaviour = assembly.MainModule.GetType("UnityEngine.MonoBehaviour");

        if (monoBehaviour == null) return;
        _patchedHarmonyEarlyPatcher = true;

        // find static ctor
        var staticCtor = monoBehaviour.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);

        // add static ctor if not found
        if (staticCtor == null)
        {
            staticCtor = new(".cctor",
                MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                assembly.MainModule.ImportReference(typeof(void)));

            monoBehaviour.Methods.Add(staticCtor);
            var il = staticCtor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ret));
        }

        // harmony early patcher must be invoked before any MonoBehaviour events are invoked
        var earlyPatcher = typeof(HarmonyEarlyPatcher);
        var invoke =
            assembly.MainModule.ImportReference(earlyPatcher.GetMethod(nameof(HarmonyEarlyPatcher.PatchHarmony)));

        var firstInstruction = staticCtor.Body.Instructions.First();
        var ilProcessor = staticCtor.Body.GetILProcessor();

        // insert call to harmony early patcher
        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, invoke));
    }
}