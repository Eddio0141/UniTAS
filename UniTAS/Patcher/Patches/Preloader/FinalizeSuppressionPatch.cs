using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class FinalizeSuppressionPatch : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllDLLs.Where(x =>
    {
        var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
        return fileWithoutExtension == null ||
               StaticCtorPatchTargetInfo.AssemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
               !StaticCtorPatchTargetInfo.AssemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
    });

    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes().Where(x => x.IsClass && !x.IsAbstract);
        foreach (var type in types)
        {
            var method = type.Methods.FirstOrDefault(x =>
                x.Name == "Finalize" && !x.HasParameters && x.ReturnType.FullName == "System.Void");
            if (method is not { HasBody: true } || method.Body.Instructions.Count == 0) continue;

            StaticLogger.Log.LogDebug($"Patching finalizer of {type.FullName}.Finalize");

            var instructions = method.Body.Instructions;
            var ilProcessor = method.Body.GetILProcessor();
            var firstInstruction = instructions.First();

            var disableFinalizeInvoke = AccessTools.PropertyGetter(typeof(FinalizeSuppressor),
                nameof(FinalizeSuppressor.DisableFinalizeInvoke));
            var disableFinalizeInvokeReference = assembly.MainModule.ImportReference(disableFinalizeInvoke);

            // basically, if DisableFinalizeInvoke is true, return
            ilProcessor.InsertBefore(firstInstruction,
                ilProcessor.Create(OpCodes.Call, disableFinalizeInvokeReference));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Brfalse, firstInstruction));
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Ret));
        }
    }
}