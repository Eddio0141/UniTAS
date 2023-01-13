using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.AsyncSceneLoadTracker;
using UniTASPlugin.Patches.PatchTypes;
using UnityEngine;

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class AsyncOperationPatch
{
    private static readonly Type _assetBundleUnloadOperation =
        AccessTools.TypeByName("UnityEngine.AssetBundleUnloadOperation");

    private static readonly Type _resourceRequest = AccessTools.TypeByName("UnityEngine.ResourceRequest");

    private static readonly ISceneLoadTracker SceneLoadTracker = Plugin.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly IAssetBundleCreateRequestTracker AssetBundleCreateRequestTracker =
        Plugin.Kernel.GetInstance<IAssetBundleCreateRequestTracker>();

    private static readonly IAssetBundleRequestTracker AssetBundleRequestTracker =
        Plugin.Kernel.GetInstance<IAssetBundleRequestTracker>();

    [HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
    private class SetAllowSceneActivation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool value, AsyncOperation __instance)
        {
            SceneLoadTracker.AllowSceneActivation(value, __instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Getter)]
    private class GetAllowSceneActivation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, AsyncOperation __instance)
        {
            __result = SceneLoadTracker.IsStalling(__instance);
            return false;
        }
    }

    // TODO probably good idea to override priority

    [HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.progress), MethodType.Getter)]
    private class progress
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref float __result, AsyncOperation __instance)
        {
            __result = SceneLoadTracker.IsStalling(__instance) ? 0.9f : 1f;
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.isDone), MethodType.Getter)]
    private class isDone
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, AsyncOperation __instance)
        {
            __result = !SceneLoadTracker.IsStalling(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOperation), "Finalize")]
    private class FinalizeAsyncOperation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(AsyncOperation __instance)
        {
            SceneLoadTracker.AsyncOperationDestruction(__instance);
        }
    }

    // AssetBundleCreateRequest for static methods, AssetBundleRequest for instance methods
    // static
    [HarmonyPatch(typeof(AssetBundle), "LoadFromFileAsync_Internal")]
    private class LoadFromFileAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string path, uint crc, ulong offset, ref object __result)
        {
            // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
            var loadFromFile_Internal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromFile_Internal",
                new[] { typeof(string), typeof(uint), typeof(ulong) });
            var loadResult = loadFromFile_Internal.GetValue(path, crc, offset);
            // create a new instance
            __result = new();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, loadResult);
            return false;
        }
    }

    // static
    [HarmonyPatch(typeof(AssetBundle), "LoadFromMemoryAsync_Internal")]
    private class LoadFromMemoryAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(byte[] binary, uint crc, ref object __result)
        {
            var loadFromMemory_Internal = Traverse.Create(typeof(AssetBundle))
                .Method("LoadFromMemory_Internal", new[] { typeof(byte[]), typeof(uint) });
            var loadResult = loadFromMemory_Internal.GetValue(binary, crc);
            __result = new();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, loadResult);
            return false;
        }
    }

    // static
    [HarmonyPatch(typeof(AssetBundle), "LoadFromStreamAsyncInternal")]
    private class LoadFromStreamAsyncInternal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize, ref object __result)
        {
            var loadFromStreamInternal = Traverse.Create(typeof(AssetBundle)).Method("LoadFromStreamInternal",
                new[] { typeof(Stream), typeof(uint), typeof(uint) });
            var loadResult = loadFromStreamInternal.GetValue(stream, crc, managedReadBufferSize);
            __result = new();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, loadResult);
            return false;
        }
    }

    // instance
    [HarmonyPatch(typeof(AssetBundle), "LoadAssetAsync_Internal")]
    private class LoadAssetAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string name, Type type, ref object __result)
        {
            var loadAsset_Internal = Traverse.Create(typeof(AssetBundle))
                .Method("LoadAsset_Internal", new[] { typeof(string), typeof(Type) });
            var loadResult = loadAsset_Internal.GetValue(name, type);
            __result = new();
            AssetBundleRequestTracker.NewAssetBundleRequest(__result, loadResult);
            return false;
        }
    }

    // instance
    [HarmonyPatch(typeof(AssetBundle), "LoadAssetWithSubAssetsAsync_Internal")]
    private class LoadAssetWithSubAssetsAsync_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string name, Type type, ref object __result)
        {
            var loadAssetWithSubAssets_Internal = Traverse.Create(typeof(AssetBundle))
                .Method("LoadAssetWithSubAssets_Internal", new[] { typeof(string), typeof(Type) });
            var loadResult = loadAssetWithSubAssets_Internal.GetValue(name, type);
            __result = new();
            AssetBundleRequestTracker.NewAssetBundleRequestMultiple(__result, loadResult);
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetBundle), "UnloadAsync")]
    private class UnloadAsync
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool unloadAllLoadedObjects, ref object __result)
        {
            var unload = Traverse.Create(typeof(AssetBundle)).Method("Unload", new[] { typeof(bool) });
            _ = unload.GetValue(unloadAllLoadedObjects);
            __result = AccessTools.CreateInstance(_assetBundleUnloadOperation);
            return false;
        }
    }

    // TODO there's no non-async alternative of this
    // private static extern AssetBundleRecompressOperation RecompressAssetBundleAsync_Internal_Injected(string inputPath, string outputPath, ref BuildCompression method, uint expectedCRC, ThreadPriority priority);

    [HarmonyPatch(typeof(AssetBundleRequest), nameof(AssetBundleRequest.asset), MethodType.Getter)]
    private class get_asset
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AssetBundleRequest __instance, ref object __result)
        {
            __result = AssetBundleRequestTracker.GetAssetBundleRequest(__instance);
            return __result == null;
        }
    }

    [HarmonyPatch(typeof(AssetBundleRequest), "allAssets", MethodType.Getter)]
    private class get_allAssets
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AssetBundleRequest __instance, ref object __result)
        {
            __result = AssetBundleRequestTracker.GetAssetBundleRequestMultiple(__instance);
            return __result == null;
        }
    }

    [HarmonyPatch(typeof(AssetBundleCreateRequest), nameof(AssetBundleCreateRequest.assetBundle),
        MethodType.Getter)]
    private class get_assetBundle
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AssetBundleCreateRequest __instance, ref object __result)
        {
            __result = AssetBundleCreateRequestTracker.GetAssetBundleCreateRequest(__instance);
            return __result == null;
        }
    }

    [HarmonyPatch(typeof(Resources), "LoadAsyncInternal")]
    private class LoadAsyncInternalPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string path, Type type, ref object __result)
        {
            // returns ResourceRequest
            // should be fine with my instance and no tinkering
            __result = AccessTools.CreateInstance(_resourceRequest);
            var resultTraverse = Traverse.Create(__result);
            _ = resultTraverse.Field("m_Path").SetValue(path);
            _ = resultTraverse.Field("m_Type").SetValue(type);
            return false;
        }
    }
}