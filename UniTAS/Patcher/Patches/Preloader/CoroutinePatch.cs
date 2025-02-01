using System.Collections;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class CoroutinePatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();
        foreach (var type in types)
        {
            if (type.IsValueType) continue;
            if (type.Interfaces.All(x => x.InterfaceType.FullName != typeof(IEnumerator).FullName)) continue;

            StaticLogger.LogDebug($"coroutine patch: patching type {type.FullName}");

            var moveNext = type.Methods.FirstOrDefault(x =>
                x.Name is "MoveNext" or "System.Collections.IEnumerator.MoveNext" &&
                x.Parameters.Count == 0);
            var current =
                type.Properties.FirstOrDefault(x => x.Name is "System.Collections.IEnumerator.Current" or "Current")
                    ?.GetMethod;

            // MoveNext
            ILCodeUtils.HookHarmony(moveNext,
                typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineMoveNextPrefix)),
                typeof(CoroutineManagerManual).GetMethod(nameof(CoroutineManagerManual.CoroutineMoveNextPostfix)));

            // current
            ILCodeUtils.HookHarmony(current,
                postfix: typeof(CoroutineManagerManual).GetMethod(
                    nameof(CoroutineManagerManual.CoroutineCurrentPostfix)));
        }
    }
}