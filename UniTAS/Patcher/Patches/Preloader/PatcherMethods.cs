using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UniTAS.Patcher.PreloadPatchUtils;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class PatchHarmonyEarly : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs { get; } =
        new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

    private bool _patchedHarmonyEarlyPatcher;

    public override void Patch(ref AssemblyDefinition assembly)
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
            Trace.Write("Adding static ctor to MonoBehaviour");
            staticCtor = new(".cctor",
                MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                assembly.MainModule.ImportReference(typeof(void)));

            monoBehaviour.Methods.Add(staticCtor);
            var il = staticCtor.Body.GetILProcessor();
            il.Append(il.Create(OpCodes.Ret));
        }

        // harmony early patcher must be invoked before any MonoBehaviour events are invoked
        var earlyPatcher = typeof(PatcherMethods);
        var invoke =
            assembly.MainModule.ImportReference(earlyPatcher.GetMethod(nameof(PatcherMethods.PatchHarmony)));

        var firstInstruction = staticCtor.Body.Instructions.First();
        var ilProcessor = staticCtor.Body.GetILProcessor();

        // insert call to harmony early patcher
        ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, invoke));
    }
}

public static class PatcherMethods
{
    private static bool _patched;

    /// <summary>
    /// A method for patching harmony before the game starts
    /// </summary>
    public static void PatchHarmony()
    {
        if (_patched)
        {
            Patcher.Logger.LogWarning("Patching harmony early twice, something invoked the static ctor twice");
            return;
        }

        _patched = true;

        Patcher.Logger.LogDebug("Patching harmony early");

        var harmony = new HarmonyLib.Harmony("dev.yuu0141.unitas.patcher");
        harmony.PatchAll();
    }
}