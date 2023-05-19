using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnityAsyncOperationTracker;
using UniTAS.Plugin.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Plugin.Patches;

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

    private static readonly ILogger Logger = Plugin.Kernel.GetInstance<ILogger>();

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

    [HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.priority), MethodType.Setter)]
    private class prioritySetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int value, ref AsyncOperation __instance)
        {
            Logger.LogDebug($"Priority set to {value} for instance hash {__instance.GetHashCode()}, skipping");
            return false;
        }
    }

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

        private static readonly MethodInfo _loadFromFile_Internal = AccessTools.Method(typeof(AssetBundle),
            "LoadFromFile_Internal",
            new[] { typeof(string), typeof(uint), typeof(ulong) });

        private static bool Prefix(string path, uint crc, ulong offset, ref AssetBundleCreateRequest __result)
        {
            // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
            var loadResult = _loadFromFile_Internal.Invoke(null, new object[] { path, crc, offset }) as AssetBundle;
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

        private static readonly MethodBase _loadFromMemoryInternal = AccessTools.Method(typeof(AssetBundle),
            "LoadFromMemory_Internal",
            new[] { typeof(byte[]), typeof(uint) });

        private static bool Prefix(byte[] binary, uint crc, ref AssetBundleCreateRequest __result)
        {
            var loadResult = _loadFromMemoryInternal.Invoke(null, new object[] { binary, crc }) as AssetBundle;
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

        private static readonly MethodBase _loadFromStreamInternal = AccessTools.Method(typeof(AssetBundle),
            "LoadFromStreamInternal",
            new[] { typeof(Stream), typeof(uint), typeof(uint) });

        private static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize,
            ref AssetBundleCreateRequest __result)
        {
            var loadResult =
                _loadFromStreamInternal.Invoke(null, new object[] { stream, crc, managedReadBufferSize }) as
                    AssetBundle;
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

        private static readonly MethodBase _loadAssetInternal = AccessTools.Method(typeof(AssetBundle),
            "LoadAsset_Internal",
            new[] { typeof(string), typeof(Type) });

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            var loadResult = _loadAssetInternal.Invoke(__instance, new object[] { name, type }) as AssetBundleRequest;
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

        private static readonly MethodBase _loadAssetWithSubAssetsInternal = AccessTools.Method(typeof(AssetBundle),
            "LoadAssetWithSubAssets_Internal",
            new[] { typeof(string), typeof(Type) });

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            var loadResult =
                _loadAssetWithSubAssetsInternal.Invoke(__instance, new object[] { name, type }) as Object[];
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

        private static readonly MethodBase _unload =
            AccessTools.Method(typeof(AssetBundle), "Unload", new[] { typeof(bool) });

        private static bool Prefix(bool unloadAllLoadedObjects, ref object __result)
        {
            _unload.Invoke(null, new object[] { unloadAllLoadedObjects });
            __result = AccessTools.CreateInstance(typeof(AssetBundle));
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

        private static bool Prefix(AssetBundleCreateRequest __instance, ref AssetBundle __result)
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

    [HarmonyPatch(typeof(AsyncOperation), "InvokeCompletionEvent")]
    private class Test
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix()
        {
            Plugin.Log.LogDebug($"AsyncOperation.InvokeCompletionEvent, {new StackTrace()}");
        }
    }
}