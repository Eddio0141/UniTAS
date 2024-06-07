using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations;
using UniTAS.Patcher.Interfaces;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Preloader;

public class FunctionCallTrace : PreloadPatcher
{
    public override IEnumerable<string> TargetDLLs => TargetPatcherDlls.AllExcludedDLLs;
    private static readonly ManualLogSource LOG = Logger.CreateLogSource("FunctionCallTrace");

    public override void Patch(ref AssemblyDefinition assembly)
    {
        // check config if loading
        var unitasConfig = File.ReadAllText(UniTASPaths.Config);

        var funcTrace = ConfigUtils.GetEntryKey(unitasConfig, nameof(Config.Sections.Debug),
            Config.Sections.Debug.FUNCTION_CALL_TRACE);

        if (funcTrace != "true") return;

        StaticLogger.LogDebug("Hooking functions for trace logging");

        var methodInvokeTrace = AccessTools.Method(typeof(FunctionCallTrace), nameof(MethodInvokeTrace));
        var methodInvokeTraceRef = assembly.MainModule.ImportReference(methodInvokeTrace);

        foreach (var type in assembly.Modules.SelectMany(module => module.GetAllTypes()))
        {
            if (type.IsEnum || type.IsInterface) continue;

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                var body = method.Body;
                body.SimplifyMacros();

                var il = body.GetILProcessor();

                var methodInvokeTraceInstruction = il.Create(OpCodes.Call, methodInvokeTraceRef);

                il.InsertBefore(body.Instructions.First(), methodInvokeTraceInstruction);

                body.OptimizeMacros();
            }
        }
    }

    public static void MethodInvokeTrace()
    {
        var stack = new StackTrace();
        var frame = stack.GetFrame(1);

        if (frame == null)
        {
            LOG.LogWarning("trace data is empty, cannot trace call");
            return;
        }

        var space = stack.FrameCount - 2;
        var func = frame.GetMethod();

        var funcNameFull = $"{func.DeclaringType.SaneFullName()}.{func.Name}";

        LOG.LogInfo($"{new string(' ', space)}{funcNameFull}");
    }
}