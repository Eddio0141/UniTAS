using System;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using ue = UnityEngine;

namespace UniTAS.Patcher.SafeAPI.UnityEngine;

public static class AssetBundle
{
    public static readonly Action<bool> UnloadAllAssetBundles =
        AccessTools.Method(typeof(ue.AssetBundle), "UnloadAllAssetBundles")?.MethodDelegate<Action<bool>>();
}