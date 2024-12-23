using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class AssetBundleTracker
{
    private static readonly IAssetBundleTracker Tracker = ContainerStarter.Kernel.GetInstance<IAssetBundleTracker>();
    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    [HarmonyPatch]
    private static class LoadMethods
    {
        private static bool Prepare()
        {
            return SafeAPI.UnityEngine.AssetBundle.UnloadAllAssetBundles == null;
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            // target methods with signatures:
            // static extern AssetBundle

            var methods = AccessTools.GetDeclaredMethods(typeof(AssetBundle))
                .Where(x => x.IsStatic &&
                            (x.ReturnType == typeof(AssetBundle) || x.ReturnType == typeof(AssetBundleCreateRequest)) &&
                            x.IsExtern())
                .Select(x => (MethodBase)x);

            foreach (var method in methods)
            {
                Logger.LogDebug($"Target method for tracking AssetBundle load: AssetBundle.{method.Name}");
                yield return method;
            }
        }

        private static void Postfix(object __result)
        {
            switch (__result)
            {
                case AssetBundle assetBundle:
                    Tracker.NewInstance(assetBundle);
                    break;
                case AssetBundleCreateRequest assetBundleCreateRequest:
                    Tracker.NewInstance(assetBundleCreateRequest);
                    break;
            }
        }
    }
}