using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.ManualServices.Trackers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class SerializationCallbackPatch : PreloadPatcher
{
    public override void Patch(ref AssemblyDefinition assembly)
    {
        var types = assembly.MainModule.GetAllTypes();

        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract || type.IsValueType) continue;

            var serializationCallback = false;
            foreach (var i in type.Interfaces)
            {
                var iType = i.InterfaceType;
                while (iType != null)
                {
                    if (iType.FullName == "UnityEngine.ISerializationCallbackReceiver")
                    {
                        serializationCallback = true;
                        break;
                    }

                    iType = iType.SafeResolve()?.BaseType;
                }

                if (serializationCallback) break;
            }

            if (!serializationCallback)
                continue;

            var method = type.Methods.FirstOrDefault(m =>
                m.Name is "OnAfterDeserialize" or "UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize");

            ILCodeUtils.HookHarmony(method,
                typeof(SerializationCallbackTrackerManual).GetMethod(nameof(SerializationCallbackTrackerManual
                    .OnAfterDeserializeInvoke)));
        }
    }
}