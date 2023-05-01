using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Patches;

[RawPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class OpenFileStreamTrackerPatch
{
    private static readonly IFileStreamTracker Tracker = Plugin.Kernel.GetInstance<IFileStreamTracker>();
    private static readonly ILogger Logger = Plugin.Kernel.GetInstance<ILogger>();

    private static bool CalledFromPlugin()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames()?.ToList();
        if (frames is { Count: >= 2 })
            frames.RemoveRange(0, 2);
        if (frames == null) return false;

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var declaringType = method?.DeclaringType;
            if (declaringType == null) continue;
            if (Equals(declaringType.Assembly, typeof(Plugin).Assembly) ||
                Equals(declaringType.Assembly, typeof(Patcher.Patcher).Assembly)) return true;
        }

        return false;
    }

    [HarmonyPatch]
    private class FileStreamCtors
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            var ctors = AccessTools.GetDeclaredConstructors(typeof(FileStream));
            foreach (var ctor in ctors)
            {
                yield return ctor;
            }
        }

        private static void Postfix(FileStream __instance)
        {
            if (CalledFromPlugin()) return;
            Tracker.NewFileStream(__instance);
            Logger.LogDebug($"New FileStream: {__instance.Name}");
        }
    }

    [HarmonyPatch(typeof(FileStream), nameof(FileStream.Dispose))]
    private class Dispose
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(FileStream __instance)
        {
            if (CalledFromPlugin()) return;
            Tracker.CloseFileStream(__instance);
            Logger.LogDebug($"Close FileStream: {__instance.Name}");
        }
    }
}