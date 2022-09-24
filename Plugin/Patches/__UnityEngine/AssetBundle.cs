using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

/*
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
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromFile_Internal", new Type[] { typeof(string), typeof(uint), typeof(ulong) });
        // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
        // AssetBundleCreateRequest for static methods, AssetBundleRequest for instance methods
        var _ = loadFromFile_Internal.GetValue(new object[] { path, crc, offset });
        // create a new instance, assign an UID to this instance, and make the override getter return a fake AssetBundle instance for UID with whats required in it
        __result = new AssetBundleCreateRequest();
        // TODO assign UID
        // TODO handle return, this returns AssetBundleCreateRequest, sort it out with AsyncOperation in mind
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

    static bool Prefix(byte[] binary, uint crc, AssetBundleCreateRequest __result)
    {
        // TODO handle return, this returns AssetBundleCreateRequest, sort it out with AsyncOperation in mind
        var loadFromMemory_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromMemory_Internal", new Type[] { typeof(byte[]), typeof(uint) });
        var _ = loadFromMemory_Internal.GetValue(new object[] { binary, crc });
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

    static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize, AssetBundleCreateRequest __result)
    {
        // TODO handle return, this returns AssetBundleCreateRequest, sort it out with AsyncOperation in mind
        var loadFromStreamInternal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromStreamInternal", new Type[] { typeof(Stream), typeof(uint), typeof(uint) });
        var _ = loadFromStreamInternal.GetValue(new object[] { stream, crc, managedReadBufferSize });
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

    static bool Prefix(string name, Type type, AssetBundleRequest __result)
    {
        // TODO handle return, this returns AssetBundleRequest, sort it out with AsyncOperation in mind
        var loadAsset_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAsset_Internal", new Type[] { typeof(string), typeof(Type) });
        var _ = loadAsset_Internal.GetValue(new object[] { name, type });
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

    static bool Prefix(string name, Type type, AssetBundleRequest __result)
    {
        // TODO handle return, this returns AssetBundleRequest, sort it out with AsyncOperation in mind
        var loadAssetWithSubAssets_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAssetWithSubAssets_Internal", new Type[] { typeof(string), typeof(Type) });
        var _ = loadAssetWithSubAssets_Internal.GetValue(new object[] { name, type });
        return false;
    }
}

// TODO theres no non-async alternative of this
// private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);

// TODO patch UnloadAsync with Unload*/