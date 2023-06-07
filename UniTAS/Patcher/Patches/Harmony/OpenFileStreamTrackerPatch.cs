using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class OpenFileStreamTrackerPatch
{
    private static readonly IFileStreamTracker Tracker = ContainerStarter.Kernel.GetInstance<IFileStreamTracker>();
    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

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
            var declaringAssembly = declaringType.Assembly;
            if (Equals(declaringAssembly, typeof(Entry).Assembly) ||
                Equals(declaringAssembly, typeof(BepInEx.Paths).Assembly)) return true;
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