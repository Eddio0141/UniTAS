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
#if TRACE
using System.Collections.Generic;
#endif

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

    private static readonly IAsyncOperationOverride AsyncOperationOverride =
        ContainerStarter.Kernel.GetInstance<IAsyncOperationOverride>();

    private static readonly IResourceAsyncTracker ResourceAsyncTracker =
        ContainerStarter.Kernel.GetInstance<IResourceAsyncTracker>();

    private static readonly IAssetBundleTracker Tracker = ContainerStarter.Kernel.GetInstance<IAssetBundleTracker>();

    private static readonly ILogger Logger = ContainerStarter.Kernel.GetInstance<ILogger>();

#if TRACE
    [HarmonyPatch]
    private class AsyncOperationReturnTrace
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static IEnumerable<MethodInfo> TargetMethods()
        {
            foreach (var type in AccessTools.AllTypes())
            {
                if (Equals(type.Assembly, typeof(AsyncOperationReturnTrace).Assembly)) continue;

                IEnumerable<MethodInfo> methods;
                try
                {
                    methods = AccessTools.GetDeclaredMethods(type);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (var method in methods)
                {
                    Type ret;
                    try
                    {
                        ret = method.ReturnType;
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (ret == typeof(AsyncOperation)) yield return method;
                }
            }
        }

        private static void Postfix(AsyncOperation __result)
        {
            StaticLogger.LogDebug($"thingy: {DebugHelp.PrintClass(__result)}, {new StackTrace()}");
        }
    }
#endif

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
            if (!AsyncOperationIsInvokingOnComplete.IsInvokingOnComplete(__instance, out var invoking)) return false;
            return !invoking;
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
    private class set_priority
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int value, ref AsyncOperation __instance)
        {
            return !AsyncOperationOverride.SetPriority(__instance, value);
        }
    }

    [HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.priority), MethodType.Getter)]
    private class get_priority
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result, ref AsyncOperation __instance)
        {
            if (!AsyncOperationOverride.GetPriority(__instance, out var priority)) return true;

            __result = priority;
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

            if (!AsyncOperationOverride.Progress(__instance, out var progress))
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

            if (!AsyncOperationOverride.IsDone(__instance, out var isDone))
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

        private static bool Prefix(IntPtr ___m_Ptr)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            return ___m_Ptr != IntPtr.Zero;
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

        private static bool Prefix(string path, uint crc, ulong offset, ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug($"Async op, load file async, path: {path}");
            StaticLogger.Trace(new StackTrace());
            __result = new AssetBundleCreateRequest();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, path, crc, offset);
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

        private static bool Prefix(byte[] binary, uint crc, ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug("Async op, load memory async");
            __result = new AssetBundleCreateRequest();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, binary, crc);
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

        private static bool Prefix(Stream stream, uint crc, uint managedReadBufferSize,
            ref AssetBundleCreateRequest __result)
        {
            StaticLogger.LogDebug("Async op, load stream async");
            __result = new AssetBundleCreateRequest();
            AssetBundleCreateRequestTracker.NewAssetBundleCreateRequest(__result, stream, crc, managedReadBufferSize);
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

        private static readonly Func<AssetBundle, string, Type, Object> LoadAssetInternal = AccessTools.Method(
            typeof(AssetBundle),
            "LoadAsset_Internal",
            [typeof(string), typeof(Type)]).MethodDelegate<Func<AssetBundle, string, Type, Object>>();

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            StaticLogger.LogDebug($"Async op, load asset async, name: {name}, type: {type.SaneFullName()}");
            // unsure why, but this one seems to completely instantly pretty much...
            var obj = LoadAssetInternal(__instance, name, type);
            __result = new AssetBundleRequest();
            AssetBundleRequestTracker.NewAssetBundleRequest(__result, obj);
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

        private static bool Prefix(AssetBundle __instance, string name, Type type, ref AssetBundleRequest __result)
        {
            StaticLogger.LogDebug(
                $"Async op, load asset with sub assets async, name: {name}, type: {type.SaneFullName()}");
            __result = new AssetBundleRequest();
            AssetBundleRequestTracker.NewAssetBundleRequestMultiple(__result, __instance, name, type);
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
            __result = AccessTools.CreateInstance(AssetBundleUnloadOperationType);
            AssetBundleTracker.UnloadBundleAsync((AsyncOperation)__result, __instance, unloadAllLoadedObjects);
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

        private static bool Prefix(AssetBundleRequest __instance, ref Object __result)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            return !AssetBundleRequestTracker.GetAssetBundleRequest(__instance, out __result);
        }
    }

    [HarmonyPatch(typeof(AssetBundleRequest), "allAssets", MethodType.Getter)]
    private class get_allAssets
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AssetBundleRequest __instance, ref Object[] __result)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            return !AssetBundleRequestTracker.GetAssetBundleRequestMultiple(__instance, out __result);
        }
    }

    [HarmonyPatch(typeof(AssetBundleCreateRequest), nameof(AssetBundleCreateRequest.assetBundle), MethodType.Getter)]
    private class get_assetBundle
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(AssetBundleCreateRequest __instance, ref AssetBundle __result)
        {
            StaticLogger.Trace($"patch prefix invoke\n{new StackTrace()}");

            return !AssetBundleCreateRequestTracker.GetAssetBundleCreateRequest(__instance, out __result);
        }
    }

    [HarmonyPatch(typeof(Resources), "LoadAsyncInternal")]
    private class LoadAsyncInternalPatch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string path, Type type, ref AsyncOperation __result)
        {
            StaticLogger.LogDebug("Resources async op, load");
            __result = (AsyncOperation)AccessTools.CreateInstance(_resourceRequest);
            ResourceAsyncTracker.ResourceLoadAsync(__result, path, type);
            return false;
        }
    }
}