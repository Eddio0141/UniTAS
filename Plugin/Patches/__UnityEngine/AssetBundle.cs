using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

class Dummy
{
    internal static extern AssetBundleCreateRequest LoadFromFileAsync_Internal(string path, uint crc, ulong offset);

    internal static extern AssetBundle LoadFromFile_Internal(string path, uint crc, ulong offset);

    internal static extern AssetBundleCreateRequest LoadFromMemoryAsync_Internal(byte[] binary, uint crc);

    internal static extern AssetBundle LoadFromMemory_Internal(byte[] binary, uint crc);

    internal static extern AssetBundleCreateRequest LoadFromStreamAsyncInternal(Stream stream, uint crc, uint managedReadBufferSize);

    internal static extern AssetBundle LoadFromStreamInternal(Stream stream, uint crc, uint managedReadBufferSize);

    private extern Object LoadAsset_Internal(string name, Type type);

    private extern AssetBundleRequest LoadAssetAsync_Internal(string name, Type type);

    internal extern Object[] LoadAssetWithSubAssets_Internal(string name, Type type);

    private extern AssetBundleRequest LoadAssetWithSubAssetsAsync_Internal(string name, Type type);

    // theres no non-async alternative of this
    // private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);
}