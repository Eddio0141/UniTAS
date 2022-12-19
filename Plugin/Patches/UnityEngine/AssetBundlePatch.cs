using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.SafeWrappers;
using ObjectOrig = UnityEngine.Object;
using AssetBundleOrig = UnityEngine.AssetBundle;
using AssetBundleCreateRequestOrig = UnityEngine.AssetBundleCreateRequest;
using AsyncOpOrig = UnityEngine.AsyncOperation;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment
// ReSharper disable CommentTypo

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class AssetBundlePatch
{
    // AssetBundleCreateRequest for static methods, AssetBundleRequest for instance methods
    // static
    [HarmonyPatch(typeof(AssetBundleOrig), "LoadFromFileAsync_Internal")]
    internal class LoadFromFileAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string path, uint crc, ulong offset, ref AssetBundleCreateRequestOrig __result)
        {
            // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
            var loadFromFile_Internal = Traverse.Create(typeof(AssetBundleOrig)).Method("LoadFromFile_Internal",
                new[] { typeof(string), typeof(uint), typeof(ulong) });
            var loadResult = loadFromFile_Internal.GetValue(path, crc, offset);
            // create a new instance, assign an UID to this instance, and make the override getter return a fake AssetBundle instance for UID with whats required in it
            __result = new();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundleOrig)loadResult);
            return false;
        }
    }

    // static
    [HarmonyPatch(typeof(AssetBundleOrig), "LoadFromMemoryAsync_Internal")]
    internal class LoadFromMemoryAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(byte[] binary, uint crc, ref AssetBundleCreateRequestOrig __result)
        {
            var loadFromMemory_Internal = Traverse.Create(typeof(AssetBundleOrig))
                .Method("LoadFromMemory_Internal", new[] { typeof(byte[]), typeof(uint) });
            var loadResult = loadFromMemory_Internal.GetValue(binary, crc);
            __result = new();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundleOrig)loadResult);
            return false;
        }
    }

    // static
    [HarmonyPatch(typeof(AssetBundleOrig), "LoadFromStreamAsyncInternal")]
    internal class LoadFromStreamAsyncInternal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize,
            ref AssetBundleCreateRequestOrig __result)
        {
            var loadFromStreamInternal = Traverse.Create(typeof(AssetBundleOrig)).Method("LoadFromStreamInternal",
                new[] { typeof(Stream), typeof(uint), typeof(uint) });
            var loadResult = loadFromStreamInternal.GetValue(stream, crc, managedReadBufferSize);
            __result = new();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundleOrig)loadResult);
            return false;
        }
    }

    // instance
    [HarmonyPatch(typeof(AssetBundleOrig), "LoadAssetAsync_Internal")]
    internal class LoadAssetAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string name, Type type, ref AssetBundleCreateRequestOrig __result)
        {
            var loadAsset_Internal = Traverse.Create(typeof(AssetBundleOrig))
                .Method("LoadAsset_Internal", new[] { typeof(string), typeof(Type) });
            var loadResult = loadAsset_Internal.GetValue(name, type);
            __result = new();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            AssetBundleRequestWrap.NewFakeInstance(wrap, (ObjectOrig)loadResult);
            return false;
        }
    }

    // instance
    [HarmonyPatch(typeof(AssetBundleOrig), "LoadAssetWithSubAssetsAsync_Internal")]
    internal class LoadAssetWithSubAssetsAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(string name, Type type, ref AssetBundleCreateRequestOrig __result)
        {
            var loadAssetWithSubAssets_Internal = Traverse.Create(typeof(AssetBundleOrig))
                .Method("LoadAssetWithSubAssets_Internal", new[] { typeof(string), typeof(Type) });
            var loadResult = loadAssetWithSubAssets_Internal.GetValue(name, type);
            __result = new();
            var wrap = new AsyncOperationWrap(__result);
            wrap.AssignUID();
            AssetBundleRequestWrap.NewFakeInstance(wrap, (ObjectOrig[])loadResult);
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetBundleOrig), "UnloadAsync")]
    internal class UnloadAsync
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(bool unloadAllLoadedObjects, ref object __result)
        {
            var unload = Traverse.Create(typeof(AssetBundleOrig)).Method("Unload", new[] { typeof(bool) });
            _ = unload.GetValue(unloadAllLoadedObjects);
            var assetBundleUnloadOperation = AccessTools.TypeByName("UnityEngine.AssetBundleUnloadOperation");
            __result = AccessTools.CreateInstance(assetBundleUnloadOperation);
            // my instance so set UID to not let the game destroy it normally
            new AsyncOperationWrap((AsyncOpOrig)__result).AssignUID();
            return false;
        }
    }

    // TODO there's no non-async alternative of this
    // private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);
}