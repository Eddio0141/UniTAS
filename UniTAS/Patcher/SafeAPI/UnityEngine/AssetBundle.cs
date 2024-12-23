using System;
using HarmonyLib;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher.SafeAPI.UnityEngine;

public static class AssetBundle
{
    public static readonly Action<bool> UnloadAllAssetBundles =
        AccessTools.Method(typeof(AssetBundle), "UnloadAllAssetBundles")?.MethodDelegate<Action<bool>>();
}