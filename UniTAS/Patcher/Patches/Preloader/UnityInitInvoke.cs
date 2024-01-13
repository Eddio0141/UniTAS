using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class UnityInitInvoke : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs { get; }

    public UnityInitInvoke()
    {
        var bepInExConfig = File.ReadAllText(Paths.BepInExConfigPath);

        const string entryPoint = "Preloader.Entrypoint";
        var foundEntryAssembly = GetEntryKey(bepInExConfig, entryPoint, "Assembly");
        var targetDLLs = foundEntryAssembly != null
            ? [foundEntryAssembly]
            : new[] { "UnityEngine.CoreModule.dll", "UnityEngine.dll" };

        // add patch target dlls too
        targetDLLs = targetDLLs.Concat(TargetPatcherDlls.AllExcludedDLLs).Distinct().ToArray();
        TargetDLLs = targetDLLs;

        _targetClass = GetEntryKey(bepInExConfig, entryPoint, "Type") ??
                       // better late than never
                       "UnityEngine.Camera";
        _targetMethod = GetEntryKey(bepInExConfig, entryPoint, "Method") ?? ".cctor";
        StaticLogger.Log.LogInfo(
            $"UniTAS will be hooked on {_targetClass}.{_targetMethod} in {string.Join(", ", targetDLLs)} as a last resort init hook");
    }

    // why can't I even load config with BepInEx's own ConfigFile????
    private static string GetEntryKey(string configRaw, string entry, string key)
    {
        var entryStart = configRaw.IndexOf($"[{entry}]", StringComparison.InvariantCulture);
        if (entryStart == -1) return null;

        var keyStart = configRaw.IndexOf(key, entryStart, StringComparison.InvariantCulture);
        if (keyStart == -1) return null;

        var valueStart = configRaw.IndexOf("=", keyStart, StringComparison.InvariantCulture);
        if (valueStart == -1) return null;

        var valueEnd = configRaw.IndexOf("\n", valueStart, StringComparison.InvariantCulture);

        return configRaw.Substring(valueStart + 1, valueEnd - valueStart - 1).Trim();
    }

    private readonly string _targetClass;
    private readonly string _targetMethod;

    public override void Patch(ref AssemblyDefinition assembly)
    {
        TryHookAwakes(assembly);
        // TryHookRuntimeInits(assembly);
        // this probably interferes with actual loading since it could initialise too early
        // TryHookLastResort(assembly);
    }

    // doing this seems to fail so Awake hooks are enough
    // private static void TryHookRuntimeInits(AssemblyDefinition assembly)
    // {
    //     StaticLogger.Log.LogDebug("Trying to hook all RuntimeInitializeOnLoadMethodAttribute methods");
    //
    //     var types = assembly.MainModule.GetAllTypes();
    //
    //     foreach (var type in types)
    //     {
    //         // find all methods with RuntimeInitializeOnLoadMethodAttribute
    //         var initMethods = type.GetMethods().Where(x =>
    //             x.CustomAttributes.Any(a =>
    //                 a.AttributeType.FullName == "UnityEngine.RuntimeInitializeOnLoadMethodAttribute"));
    //
    //         foreach (var initMethod in initMethods)
    //         {
    //             ILCodeUtils.MethodInvokeHook(assembly, initMethod,
    //                 AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
    //             LogHook(assembly, type.Name, initMethod.Name);
    //         }
    //     }
    // }

    private static void TryHookAwakes(AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Trying to hook all MonoBehaviours Awake methods");

        var types = assembly.MainModule.GetAllTypes().Where(x => x.IsMonoBehaviour());

        foreach (var type in types)
        {
            var awake = type.GetMethods().FirstOrDefault(x => x.Name == "Awake" && !x.IsStatic && !x.HasParameters);
            if (awake == null) continue;

            ILCodeUtils.MethodInvokeHook(assembly, awake,
                AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
            LogHook(assembly, type.Name, "Awake");
        }
    }

    /*
    private void TryHookLastResort(AssemblyDefinition assembly)
    {
        StaticLogger.Log.LogDebug("Trying to hook last resort init method defined in BepInEx config");

        // make sure it exists
        var targetType = assembly.MainModule.GetAllTypes().FirstOrDefault(t => t.Name == _targetClass);
        if (targetType == null) return;

        if (_targetMethod == ".cctor")
        {
            ILCodeUtils.MethodInvokeHookOnCctor(assembly, targetType,
                AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
            LogHook(assembly, _targetClass, ".cctor");
            return;
        }

        // find method and hook
        var method = targetType.GetMethods().FirstOrDefault(m => m.Name == _targetMethod);
        if (method == null) return;

        LogHook(assembly, _targetClass, _targetMethod);

        ILCodeUtils.MethodInvokeHook(assembly, method,
            AccessTools.Method(typeof(InvokeTracker), nameof(InvokeTracker.OnUnityInit)));
    }
    */

    private static void LogHook(AssemblyDefinition assembly, string type, string method)
    {
        StaticLogger.Log.LogDebug(
            $"Adding UniTAS init hook for assembly {assembly.Name.Name} to method {type}.{method}");
    }
}