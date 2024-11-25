using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations.Customization;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class FunctionCallTrace : PreloadPatcher
{
    private static readonly ManualLogSource LOG = Logger.CreateLogSource("FunctionCallTrace");

    public override void Patch(ref AssemblyDefinition assembly)
    {
        if (!File.Exists(UniTASPaths.ConfigBepInEx)) return;

        // check config if loading
        var unitasConfig = File.ReadAllText(UniTASPaths.ConfigBepInEx);

        var funcTraceEnable = ConfigUtils.GetEntryKey(unitasConfig,
            Config.Sections.Debug.FunctionCallTrace.SectionName,
            Config.Sections.Debug.FunctionCallTrace.Enable);

        if (funcTraceEnable != "true") return;

        StaticLogger.LogDebug("Hooking functions for trace logging");

        var methodInvokeTraceStart = AccessTools.Method(typeof(FunctionCallTrace), nameof(MethodInvokeStart));
        var methodInvokeTraceStartRef = assembly.MainModule.ImportReference(methodInvokeTraceStart);
        var methodInvokeTraceEnd = AccessTools.Method(typeof(FunctionCallTrace), nameof(MethodInvokeEnd));
        var methodInvokeTraceEndRef = assembly.MainModule.ImportReference(methodInvokeTraceEnd);

        var types = assembly.Modules.SelectMany(module => module.GetAllTypes());
        var methodsHookConfig = ConfigUtils.GetEntryKey(unitasConfig,
            Config.Sections.Debug.FunctionCallTrace.SectionName,
            Config.Sections.Debug.FunctionCallTrace.Methods);

        if (methodsHookConfig == null || methodsHookConfig.Trim() == "") return;

        var hookTypeAndMethods = methodsHookConfig.Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().Split([":"], StringSplitOptions.None)).Where(x => x.Length == 2)
            .Select(x => (x[0], x[1])).GroupBy(x => x.Item1)
            .ToDictionary(x => x.Key, x => new HashSet<string>(x.Select(y => y.Item2)));

        types = types.Where(t => hookTypeAndMethods.ContainsKey(t.FullName));

        var hookedMethods = new HashSet<MethodDefinition>();
        foreach (var type in types)
        {
            var typeFullName = type.FullName;

            if (type.IsEnum || type.IsInterface)
            {
                StaticLogger.LogWarning(
                    $"Function call trace: skipping hook on type that is enum, or interface: {typeFullName}");
                continue;
            }

            StaticLogger.LogDebug($"hooking on class {typeFullName}");

            var methodsToHook = hookTypeAndMethods[typeFullName];

            foreach (var method in type.Methods)
            {
                if (!methodsToHook.Contains(method.Name)) continue;
                if (!method.HasBody)
                {
                    StaticLogger.LogWarning($"Function call trace: cannot hook method {method.Name}, it has no body");
                    continue;
                }

                var body = method.Body;
                // start hook
                var il = body.GetILProcessor();
                il.InsertBefore(body.Instructions.First(), il.Create(OpCodes.Call, methodInvokeTraceStartRef));

                // end hooks
                HookOnMethod(method, methodInvokeTraceEndRef, hookedMethods);
            }
        }
    }

    private static void HookOnMethod(MethodDefinition target, MethodReference methodInvokeTraceEndRef,
        HashSet<MethodDefinition> alreadyHooked)
    {
        if (!target.HasBody || alreadyHooked.Contains(target)) return;
        alreadyHooked.Add(target);

        StaticLogger.LogDebug($"Function call trace: hooking on method {target.FullName}");

        var body = target.Body;
        body.SimplifyMacros();

        var il = body.GetILProcessor();

        // calls
        foreach (var inst in body.Instructions)
        {
            if (inst.Operand is not MethodReference method) continue;
            // TODO: for now, we exclude System namespace
            var ns = method.DeclaringType.Namespace;
            if (ns == "System" || ns.StartsWith("System.")) continue;

            var methodDef = method.SafeResolve();
            if (methodDef == null) continue;
            HookOnMethod(methodDef, methodInvokeTraceEndRef, alreadyHooked);
        }

        // ends
        var methodInvokeTraceEndInstruction = il.Create(OpCodes.Call, methodInvokeTraceEndRef);
        for (var i = 0; i < body.Instructions.Count; i++)
        {
            var inst = body.Instructions[i];
            if (inst.OpCode != OpCodes.Ret) continue;
            il.InsertBefore(inst, methodInvokeTraceEndInstruction);
            i++;
        }

        body.OptimizeMacros();
    }

    private static readonly ThreadLocal<(int startDepth, int depth, string path)> MethodTracePath = new();

    public static void MethodInvokeStart()
    {
        if (MethodTracePath.Value.startDepth > 0) return;
        MethodTracePath.Value = (new StackTrace().FrameCount - 1, default, default);
    }

    public static void MethodInvokeEnd()
    {
        var stack = new StackTrace();
        var frames = stack.GetFrames();
        if (frames == null)
        {
            StaticLogger.LogWarning("Stack trace data is empty, cannot trace call");
            return;
        }

        var frameCount = frames.Length;
        var prevDepth = MethodTracePath.Value.depth;
        foreach (var frame in frames.Skip(1 + prevDepth).Reverse())
        {
            var func = frame.GetMethod();
            var funcNameFull = $"{func.DeclaringType.SaneFullName()}.{func.Name}";
            if (MethodTracePath.Value.path != null)
            {
                funcNameFull = $"{MethodTracePath.Value.path} -> {funcNameFull}";
            }

            // -2, since 1 for this method, and another since we are handling the return trace
            MethodTracePath.Value = (MethodTracePath.Value.startDepth, 0, funcNameFull);
        }

        // add return, if anything to be returned to
        if (frameCount - MethodTracePath.Value.startDepth > 1)
        {
            var retMethod = frames[2].GetMethod();
            var funcNameFull = $" <- {retMethod.DeclaringType.SaneFullName()}.{retMethod.Name}";
            MethodTracePath.Value = (MethodTracePath.Value.startDepth, frameCount - 2,
                MethodTracePath.Value.path + funcNameFull);
            return;
        }

        // dump the result
        LOG.LogInfo(MethodTracePath.Value.path);
        MethodTracePath.Value = default;
    }
}