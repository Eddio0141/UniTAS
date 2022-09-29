using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

// AssetBundleCreateRequest for static methods, AssetBundleRequest for instance methods
// static
[HarmonyPatch(typeof(AssetBundle), "LoadFromFileAsync_Internal")]
class LoadFromFileAsync_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string path, uint crc, ulong offset, ref AssetBundleCreateRequest __result)
    {
        // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromFile_Internal", new Type[] { typeof(string), typeof(uint), typeof(ulong) });
        var loadResult = loadFromFile_Internal.GetValue(new object[] { path, crc, offset });
        // create a new instance, assign an UID to this instance, and make the override getter return a fake AssetBundle instance for UID with whats required in it
        __result = new AssetBundleCreateRequest();
        var wrap = new AsyncOperationWrap(__result);
        wrap.AssignUID();
        AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundle)loadResult);
        return false;
    }
}

// static
[HarmonyPatch(typeof(AssetBundle), "LoadFromMemoryAsync_Internal")]
class LoadFromMemoryAsync_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(byte[] binary, uint crc, ref AssetBundleCreateRequest __result)
    {
        var loadFromMemory_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromMemory_Internal", new Type[] { typeof(byte[]), typeof(uint) });
        var loadResult = loadFromMemory_Internal.GetValue(new object[] { binary, crc });
        __result = new AssetBundleCreateRequest();
        var wrap = new AsyncOperationWrap(__result);
        wrap.AssignUID();
        AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundle)loadResult);
        return false;
    }
}

// static
[HarmonyPatch(typeof(AssetBundle), "LoadFromStreamAsyncInternal")]
class LoadFromStreamAsyncInternal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize, ref AssetBundleCreateRequest __result)
    {
        var loadFromStreamInternal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromStreamInternal", new Type[] { typeof(Stream), typeof(uint), typeof(uint) });
        var loadResult = loadFromStreamInternal.GetValue(new object[] { stream, crc, managedReadBufferSize });
        __result = new AssetBundleCreateRequest();
        var wrap = new AsyncOperationWrap(__result);
        wrap.AssignUID();
        AssetBundleCreateRequestWrap.NewFakeInstance(wrap, (AssetBundle)loadResult);
        return false;
    }
}

// instance
[HarmonyPatch(typeof(AssetBundle), "LoadAssetAsync_Internal")]
class LoadAssetAsync_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string name, Type type, ref AssetBundleRequest __result)
    {
        var loadAsset_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAsset_Internal", new Type[] { typeof(string), typeof(Type) });
        var loadResult = loadAsset_Internal.GetValue(new object[] { name, type });
        __result = new AssetBundleRequest();
        var wrap = new AsyncOperationWrap(__result);
        wrap.AssignUID();
        AssetBundleRequestWrap.NewFakeInstance(wrap, (UnityEngine.Object)loadResult);
        return false;
    }
}

// instance
[HarmonyPatch(typeof(AssetBundle), "LoadAssetWithSubAssetsAsync_Internal")]
class LoadAssetWithSubAssetsAsync_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string name, Type type, ref AssetBundleRequest __result)
    {
        var loadAssetWithSubAssets_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAssetWithSubAssets_Internal", new Type[] { typeof(string), typeof(Type) });
        var loadResult = loadAssetWithSubAssets_Internal.GetValue(new object[] { name, type });
        __result = new AssetBundleRequest();
        var wrap = new AsyncOperationWrap(__result);
        wrap.AssignUID();
        AssetBundleRequestWrap.NewFakeInstance(wrap, (UnityEngine.Object[])loadResult);
        return false;
    }
}

[HarmonyPatch(typeof(AssetBundle), "UnloadAsync")]
class UnloadAsync
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(bool unloadAllLoadedObjects, ref object __result)
    {
        var unload = Traverse.Create(typeof(AssetBundle)).Method("Unload", new Type[] { typeof(bool) });
        _ = unload.GetValue(new object[] { unloadAllLoadedObjects });
        var assetBundleUnloadOperation = AccessTools.TypeByName("UnityEngine.AssetBundleUnloadOperation");
        __result = AccessTools.CreateInstance(assetBundleUnloadOperation);
        // my instance so set UID to not let the game destroy it normally
        new AsyncOperationWrap((AsyncOperation)__result).AssignUID();
        return false;
    }
}

// TODO theres no non-async alternative of this
// private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);
