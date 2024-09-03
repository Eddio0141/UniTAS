using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ScriptableObjectPatch
{
    private static readonly INewScriptableObjectTracker NewScriptableObjectTracker =
        ContainerStarter.Kernel.GetInstance<INewScriptableObjectTracker>();

    [HarmonyPatch]
    private class CreateInstanceTrackerString
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ScriptableObject), nameof(ScriptableObject.CreateInstance),
                [typeof(string)]);
            yield return AccessTools.Method(typeof(ScriptableObject), nameof(ScriptableObject.CreateInstance),
                [typeof(Type)]);
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(ScriptableObject __result)
        {
            var st = new StackTrace();
            var frames = st.GetFrames()?.Skip(2).ToArray();
            if (frames == null) return;

            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                var declType = method.DeclaringType;

                if (declType?.Namespace == "UnityEngine.InputSystem")
                {
                    StaticLogger.Log.LogInfo(
                        $"ScriptableObject.CreateInstance was invoked from {declType.SaneFullName()}.{method.Name}, skipping tracking this instance");
                    return;
                }
            }

            NewScriptableObjectTracker.NewScriptableObject(__result);
        }
    }
}