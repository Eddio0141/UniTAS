using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(AssetBundle), "LoadFromFileAsync_Internal")]
class LoadFromFileAsync_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string path, uint crc, ulong offset, AssetBundleCreateRequest __result)
    {
        // TODO handle return, this returns AssetBundleCreateRequest, sort it out with AsyncOperation in mind
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromFile_Internal", new Type[] { typeof(string), typeof(uint), typeof(ulong) });
        var _ = loadFromFile_Internal.GetValue(new object[] { path, crc, offset });
        return false;
    }
}

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
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromMemory_Internal", new Type[] { typeof(byte[]), typeof(uint) });
        var _ = loadFromFile_Internal.GetValue(new object[] { binary, crc });
        return false;
    }
}

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
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromStreamInternal", new Type[] { typeof(Stream), typeof(uint), typeof(uint) });
        var _ = loadFromFile_Internal.GetValue(new object[] { stream, crc, managedReadBufferSize });
        return false;
    }
}

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
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAsset_Internal", new Type[] { typeof(string), typeof(Type) });
        var _ = loadFromFile_Internal.GetValue(new object[] { name, type });
        return false;
    }
}

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
        var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadAssetWithSubAssets_Internal", new Type[] { typeof(string), typeof(Type) });
        var _ = loadFromFile_Internal.GetValue(new object[] { name, type });
        return false;
    }
}

// TODO theres no non-async alternative of this
// private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);