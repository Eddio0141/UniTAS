using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityAsyncOperationTracker;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class AsyncOperationPatch
{
    private static readonly Type _assetBundleUnloadOperation =
        AccessTools.TypeByName("UnityEngine.AssetBundleUnloadOperation");

    private static readonly Type _resourceRequest = AccessTools.TypeByName("UnityEngine.ResourceRequest");

    private static readonly ISceneLoadTracker SceneLoadTracker =
        ContainerStarter.Kernel.GetInstance<ISceneLoadTracker>();

    private static readonly IAssetBundleCreateRequestTracker AssetBundleCreateRequestTracker =
        ContainerStarter.Kernel.GetInstance<IAssetBundleCreateRequestTracker>();

    private static readonly IAssetBundleRequestTracker AssetBundleRequestTracker =
        ContainerStarter.Kernel.GetInstance<IAssetBundleRequestTracker>();

    private static readonly IAsyncOperationIsInvokingOnComplete AsyncOperationIsInvokingOnComplete =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationIsInvokingOnComplete>();

    private static readonly IAssetBundleTracker AssetBundleTracker =
        ContainerStarter.Kernel.GetInstance<IAssetBundleTracker>();

    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

    [HarmonyPatch(typeof(AsyncOperation), "InvokeCompletionEvent")]
    private class InvokeCompletionEvent
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AsyncOperation __instance)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");
            return AsyncOperationIsInvokingOnComplete.IsInvokingOnComplete(__instance, out var invoking) && invoking;
        }
    }

    [HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
    private class SetAllowSceneActivation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool value, AsyncOperation __instance)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");
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
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");
            if (!SceneLoadTracker.GetAllowSceneActivation(__instance, out var state))
            {
                return true;
            }

            __result = state;
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
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");
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
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            // behaviour on AsyncOperation returned from scene operation is
            // 0.9f if the scene is loaded and ready, doesn't matter if allowSceneActivation is true,
            // it will be at 0.9 till the scene loads in the next frame

            if (!SceneLoadTracker.Progress(__instance, out var progress))
            {
                // not tracked instance, proceed as original
                return true;
            }

            __result = progress;
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
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            if (!SceneLoadTracker.IsDone(__instance, out var isDone))
            {
                StaticLogger.Trace("didn't find a tracked AsyncOperation instance");
                return true;
            }

            StaticLogger.Trace($"result = {isDone}");
            __result = isDone;
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

        private static bool Prefix(AsyncOperation __instance)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            var instanceTraverse = new Traverse(__instance).Field("m_Ptr");
            return instanceTraverse.GetValue<IntPtr>() != IntPtr.Zero;
        }
    }

    // AssetBundleCreateRequest for static methods, AssetBundleRequest for instance methods

    // static
    [HarmonyPatch]
    private class LoadFromFileAsync
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodInfo TargetMethod()
        {
            var internalMethod = AccessTools.Method(typeof(AssetBundle), "LoadFromFileAsync_Internal",
                [typeof(string), typeof(uint), typeof(ulong)]);

            if (internalMethod != null)
                return internalMethod;

            return AccessTools.Method(typeof(AssetBundle), "LoadFromFileAsync",
                [typeof(string), typeof(uint), typeof(ulong)]);
        }

        private static readonly MethodInfo _loadFromFile =
            AccessTools.Method(typeof(AssetBundle), "LoadFromFile_Internal",
                [typeof(string), typeof(uint), typeof(ulong)]) ??
            AccessTools.Method(typeof(AssetBundle), "LoadFromFile",
                [typeof(string), typeof(uint), typeof(ulong)]);

        private static bool Prefix(string path, uint crc, ulong offset, ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug($"Async op, load file async, path: {path}");

            // LoadFromFile fails with null return if operation fails, __result.assetBundle will also reflect that if async load fails too
            var loadResult = _loadFromFile.Invoke(null, [path, crc, offset]) as AssetBundle;
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
            [typeof(byte[]), typeof(uint)]);

        private static bool Prefix(byte[] binary, uint crc, ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug("Async op, load memory async");

            var loadResult = _loadFromMemoryInternal.Invoke(null, [binary, crc]) as AssetBundle;
            __result = new();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, loadResult);
            return false;
        }
    }

    // static
    [HarmonyPatch(typeof(AssetBundle), "LoadFromStreamAsyncInternal", typeof(Stream), typeof(uint), typeof(uint))]
    private class LoadFromStreamAsyncInternal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static readonly MethodBase _loadFromStreamInternal = AccessTools.Method(typeof(AssetBundle),
            "LoadFromStreamInternal",
            [typeof(Stream), typeof(uint), typeof(uint)]);

        private static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize,
            ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug("Async op, load stream async");

            var loadResult =
                _loadFromStreamInternal.Invoke(null, [stream, crc, managedReadBufferSize]) as
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
            [typeof(string), typeof(Type)]);

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            StaticLogger.LogDebug($"Async op, load asset async, name: {name}, type: {type.SaneFullName()}");

            var loadResult = _loadAssetInternal.Invoke(__instance, [name, type]) as Object;
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
            [typeof(string), typeof(Type)]);

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            StaticLogger.LogDebug(
                $"Async op, load asset with sub assets async, name: {name}, type: {type.SaneFullName()}");

            var loadResult =
                _loadAssetWithSubAssetsInternal.Invoke(__instance, [name, type]) as Object[];
            __result = new();
            // TODO: do i track scenes from those
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

        private static readonly Type AssetBundleUnloadOperationType =
            AccessTools.TypeByName("UnityEngine.AssetBundleUnloadOperation");

        private static bool Prefix(AssetBundle __instance, bool unloadAllLoadedObjects, ref object __result)
        {
            StaticLogger.LogDebug("Async op, unload AssetBundle");

            __instance.Unload(unloadAllLoadedObjects);
            __result = AccessTools.CreateInstance(AssetBundleUnloadOperationType);
            AssetBundleTracker.UnloadAsync((AsyncOperation)__result);
            return false;
        }
    }

    [HarmonyPatch(typeof(AssetBundle), nameof(AssetBundle.Unload))]
    private class Unload
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Postfix(AssetBundle __instance)
        {
            StaticLogger.LogDebug("non async op, unload AssetBundle");
            AssetBundleTracker.Unload(__instance);
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
            return false;
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
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            __result = AssetBundleCreateRequestTracker.GetAssetBundleCreateRequest(__instance);
            return false;
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
            StaticLogger.LogDebug("Resources async op, load");

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