using System.Linq;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class AsyncOperationPatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        // target:
        // public unsafe static UnityEngine.Object.AsyncInstantiateOperation<T> InstantiateAsync<T>
        //     (T original, int count, ReadOnlySpan<Vector3> positions, ReadOnlySpan<Quaternion> rotations,
        //      InstantiateParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        //   where T : Object

        if (assembly.Name.Name != "UnityEngine.CoreModule") return;

        var type = assembly.MainModule.GetAllTypes().FirstOrDefault(x => x.FullName == "UnityEngine.Object");
        var method = type?.Methods.FirstOrDefault(x =>
            x.Name == "InstantiateAsync" && x.Parameters.Count == 6
                                         && x.Parameters[0].ParameterType.FullName == "T" &&
                                         x.Parameters[1].ParameterType == x.Module.TypeSystem.Int32 &&
                                         x.Parameters[2].ParameterType.FullName ==
                                         "System.ReadOnlySpan<UnityEngine.Vector3>" &&
                                         x.Parameters[3].ParameterType.FullName ==
                                         "System.ReadOnlySpan<UnityEngine.Quaternion>" &&
                                         x.Parameters[4].ParameterType.FullName ==
                                         "UnityEngine.InstantiateParameters" &&
                                         x.Parameters[5].ParameterType.FullName ==
                                         "System.Threading.CancellationToken");
        if (method == null)
        {
            StaticLogger.LogDebug("UnityEngine.Object.AsyncInstantiateOperation patch won't apply");
            return;
        }

        StaticLogger.LogDebug("found UnityEngine.Object.AsyncInstantiateOperation");
        var prefix = AccessTools.Method(typeof(ObjectInitializeTrackerManual),
            nameof(ObjectInitializeTrackerManual.Initialize));
        ILCodeUtils.HookHarmony(method, prefix);
    }
}