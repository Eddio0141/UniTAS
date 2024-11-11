using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.ContainerBindings.GameExecutionControllers;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class FinalizeSuppressionPatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes().Where(x => x.IsClass && !x.IsAbstract);
        foreach (var type in types)
        {
            // special case
            if (type.FullName is "UnityEngine.AnimationCurve.AnimationCurve" or "UnityEngine.AnimationCurve") continue;

            var method = type.Methods.FirstOrDefault(x =>
                x.Name == "Finalize" && !x.HasParameters && x.ReturnType.FullName == "System.Void");
            if (method is not { HasBody: true }) continue;

            StaticLogger.Log.LogDebug($"Patching finalizer of {type.FullName}.Finalize");

            method.Body.SimplifyMacros();

            var instructions = method.Body.Instructions;
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = instructions.First();

            var disableFinalizeInvoke = AccessTools.PropertyGetter(typeof(FinalizeSuppressor),
                nameof(FinalizeSuppressor.DisableFinalizeInvoke));
            var disableFinalizeInvokeReference = assembly.MainModule.ImportReference(disableFinalizeInvoke);

            // basically, if DisableFinalizeInvoke is true, return

            // also this doesn't need the try-catch block
            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Call, disableFinalizeInvokeReference));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ret));

            method.Body.OptimizeMacros();
        }
    }
}