using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using UniTAS.Patcher.PreloadPatchUtils;
using UniTAS.Patcher.Runtime;

namespace UniTAS.Patcher.Patches.Preloader;

public class AudioListenerOnAudioFilterRead : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => new[]
        { "UnityEngine.AudioModule.dll", "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

    private bool _patched;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        if (_patched) return;

        // find AudioListener
        var audioListener = assembly.MainModule?.GetType("UnityEngine.AudioListener");

        if (audioListener == null) return;
        _patched = true;

        // find OnAudioFilterRead
        var onAudioFilterRead = audioListener.FindMethod("OnAudioFilterRead");
        if (onAudioFilterRead != null) return;

        // add OnAudioFilterRead
        var onAudioFilterReadMethod = new MethodDefinition("OnAudioFilterRead",
            MethodAttributes.Private | MethodAttributes.HideBySig,
            assembly.MainModule.TypeSystem.Void);

        onAudioFilterReadMethod.Parameters.Add(new("data", ParameterAttributes.None,
            assembly.MainModule.ImportReference(typeof(float[]))));

        onAudioFilterReadMethod.Parameters.Add(new("channels", ParameterAttributes.None,
            assembly.MainModule.ImportReference(typeof(int))));

        // add call to Events.OnAudioFilterRead with null check
        onAudioFilterReadMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        onAudioFilterReadMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
        onAudioFilterReadMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            assembly.MainModule.ImportReference(typeof(Events).GetMethod(nameof(Events.OnAudioFilterReadInvoke)))));

        audioListener.Methods.Add(onAudioFilterReadMethod);
    }
}