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

        // target old:
        // public static unsafe AsyncInstantiateOperation<T> InstantiateAsync<T>(
        //   T original,
        //   int count,
        //   Transform parent,
        //   ReadOnlySpan<Vector3> positions,
        //   ReadOnlySpan<Quaternion> rotations)
        //   where T : Object

        if (assembly.Name.Name != "UnityEngine.CoreModule") return;

        var type = assembly.MainModule.GetAllTypes().FirstOrDefault(x => x.FullName == "UnityEngine.Object");
        if (type == null)
        {
            StaticLogger.LogDebug("UnityEngine.Object.AsyncInstantiateOperation patch won't apply");
            return;
        }

        var method = type.Methods.FirstOrDefault(x =>
            x.Name == "InstantiateAsync" && x.Parameters.Count == 6 && x.Parameters[0].ParameterType.FullName == "T" &&
            x.Parameters[1].ParameterType == x.Module.TypeSystem.Int32 &&
            x.Parameters[2].ParameterType.FullName == "System.ReadOnlySpan`1<UnityEngine.Vector3>" &&
            x.Parameters[3].ParameterType.FullName == "System.ReadOnlySpan`1<UnityEngine.Quaternion>" &&
            x.Parameters[4].ParameterType.FullName == "UnityEngine.InstantiateParameters" &&
            x.Parameters[5].ParameterType.FullName == "System.Threading.CancellationToken"
        );
        if (method != null)
        {
            var prefix = AccessTools.GetDeclaredMethods(typeof(ObjectAsyncInitTrackerManual)).First(x =>
                x.Name == nameof(ObjectAsyncInitTrackerManual.Instantiate) && x.GetParameters().Length == 7);
            StaticLogger.LogDebug("found UnityEngine.Object.AsyncInstantiateOperation");
            ILCodeUtils.HookHarmony(method, prefix);
            return;
        }

        method = type.Methods.FirstOrDefault(x =>
            x.Name == "InstantiateAsync" && x.Parameters.Count == 5 && x.Parameters[0].ParameterType.FullName == "T" &&
            x.Parameters[1].ParameterType == x.Module.TypeSystem.Int32 &&
            x.Parameters[2].ParameterType.FullName == "UnityEngine.Transform" &&
            x.Parameters[3].ParameterType.FullName == "System.ReadOnlySpan`1<UnityEngine.Vector3>" &&
            x.Parameters[4].ParameterType.FullName == "System.ReadOnlySpan`1<UnityEngine.Quaternion>"
        );
        if (method != null)
        {
            var prefix = AccessTools.GetDeclaredMethods(typeof(ObjectAsyncInitTrackerManual)).First(x =>
                x.Name == nameof(ObjectAsyncInitTrackerManual.Instantiate) && x.GetParameters().Length == 6);
            StaticLogger.LogDebug("found UnityEngine.Object.AsyncInstantiateOperation");
            ILCodeUtils.HookHarmony(method, prefix);
            return;
        }

        StaticLogger.LogDebug("UnityEngine.Object.AsyncInstantiateOperation patch won't apply");
    }
}
