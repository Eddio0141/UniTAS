using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class UnityInitInvoke : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs { get; } =
        new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

    private readonly string[] _targetClasses =
    {
        "UnityEngine.MonoBehaviour",
        "UnityEngine.Application",
        "UnityEngine.Camera"
    };

    public override void Patch(ref AssemblyDefinition assembly)
    {
        foreach (var targetClass in _targetClasses)
        {
            // find type
            var targetType = assembly.MainModule.GetType(targetClass);

            if (targetType == null) return;
            StaticLogger.Log.LogDebug($"Found target class {targetClass} for checking unity init invoke");

            // find static ctor
            var staticCtor = targetType.Methods.FirstOrDefault(m => m.IsConstructor && m.IsStatic);

            // add static ctor if not found
            if (staticCtor == null)
            {
                StaticLogger.Log.LogDebug($"Adding cctor to {targetClass}");
                staticCtor = new(".cctor",
                    MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.HideBySig
                    | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    assembly.MainModule.ImportReference(typeof(void)));

                targetType.Methods.Add(staticCtor);
                var il = staticCtor.Body.GetILProcessor();
                il.Append(il.Create(OpCodes.Ret));
            }

            var earlyPatcher = typeof(InvokeTracker);
            var invoke =
                assembly.MainModule.ImportReference(earlyPatcher.GetMethod(nameof(InvokeTracker.OnUnityInit)));

            var firstInstruction = staticCtor.Body.Instructions.First();
            var ilProcessor = staticCtor.Body.GetILProcessor();

            // insert call before first instruction
            ilProcessor.InsertBefore(firstInstruction, ilProcessor.Create(OpCodes.Call, invoke));
        }
    }
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class InvokeTracker
{
    private static bool _invoked;

    public static void OnUnityInit()
    {
        if (_invoked) return;
        _invoked = true;

        StaticLogger.Log.LogDebug("Unity has been initialized");

        InvokeEventAttributes.Invoke<InvokeOnUnityInitAttribute>();
    }
}