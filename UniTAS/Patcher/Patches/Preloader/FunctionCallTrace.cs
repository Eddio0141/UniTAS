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
        if (!File.Exists(UniTASPaths.Config)) return;

        // check config if loading
        var unitasConfig = File.ReadAllText(UniTASPaths.Config);

        var funcTraceEnable = ConfigUtils.GetEntryKey(unitasConfig, Config.Sections.Debug.FunctionCallTrace.SECTION_NAME,
            Config.Sections.Debug.FunctionCallTrace.ENABLE);

        if (funcTraceEnable != "true") return;

        StaticLogger.LogDebug("Hooking functions for trace logging");

        var methodInvokeTrace = AccessTools.Method(typeof(FunctionCallTrace), nameof(MethodInvokeTrace));
        var methodInvokeTraceRef = assembly.MainModule.ImportReference(methodInvokeTrace);

        var types = assembly.Modules.SelectMany(module => module.GetAllTypes());
        var matchingTypesConfig = ConfigUtils.GetEntryKey(unitasConfig, Config.Sections.Debug.FunctionCallTrace.SECTION_NAME, Config.Sections.Debug.FunctionCallTrace.MATCHING_TYPES);

        if (matchingTypesConfig != null && matchingTypesConfig.Trim() != "*")
        {
            var matchingTypes = matchingTypesConfig.Split([','], System.StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            types = types.Where(t => matchingTypes.Any(x => t.FullName.Like(x)));
        }

        foreach (var type in types)
        {
            if (type.IsEnum || type.IsInterface) continue;

            StaticLogger.LogDebug($"hooking on class {type.FullName}");

            foreach (var method in type.Methods)
            {
                if (!method.HasBody) continue;

                StaticLogger.LogDebug($"hooking on method {method.Name}");

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
