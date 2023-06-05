using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Mono.Cecil;
using UniTAS.Patcher.Interfaces;
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

            ILCodeUtils.MethodInvokeHookOnCctor(assembly, targetType,
                AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
        }
    }
}