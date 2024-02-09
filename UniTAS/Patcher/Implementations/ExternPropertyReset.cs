using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using MonoMod.Utils;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Utils;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class ExternPropertyReset(ILogger logger, IPatchReverseInvoker patchReverseInvoker)
    : IOnAwakeUnconditional, IOnPreGameRestart
{
    private List<(MethodBase, object, string)> _externMethodSaves;

    public void OnPreGameRestart()
    {
        foreach (var (setMethod, value, name) in _externMethodSaves)
        {
            logger.LogDebug($"Resetting extern property: {name}");
            LoggingUtils.DiskLogger.Flush();
            var valueClone = DeepCopy.MakeDeepCopy(value);
            patchReverseInvoker.Invoke(() => setMethod.Invoke(null, [valueClone]));
        }
    }

    public void AwakeUnconditional()
    {
        if (_externMethodSaves != null) return;
        _externMethodSaves = [];

        var assembliesPath = Paths.ManagedPath;
        var assembliesPaths = Directory.GetFiles(assembliesPath, "*.dll", SearchOption.TopDirectoryOnly);
        var assemblies = assembliesPaths.Select(AssemblyDefinition.ReadAssembly);
        var types = assemblies.SelectMany(x => x.MainModule.GetAllTypes()).Where(x => x.IsClass);
        var properties = types.SelectMany(x => x.Properties).Select(x => (x, x.GetMethod, x.SetMethod))
            .Where(methods =>
            {
                var method = methods.GetMethod ?? methods.SetMethod;
                return method.IsStatic && (method.ImplAttributes & MethodImplAttributes.InternalCall) != 0;
            });

        foreach (var (propDef, getMethod, setMethod) in properties)
        {
            var typeName = propDef.DeclaringType.FullName ?? "unknown_type";
            var fullName = $"{typeName}.{propDef.Name}";

            if (_skipProperties.Any(x => fullName.Like(x))) continue;

            var get = getMethod?.ResolveReflection();
            var set = setMethod?.ResolveReflection();

            if (!_knownProperties.Contains(fullName))
            {
                logger.LogWarning(
                    $"Found unknown extern property: {fullName}, get exists: {get != null}, set exists: {set != null} ");
            }

            // right now, we only support extern properties that have both get and set
            // this could be included explicitly in the future with a list, but this is fine until set needs to be handled
            if (get == null || set == null) continue;

            // TODO this needs to be removed before merge
            // if (!fullName.StartsWith("UnityEngine.Time") || !fullName.StartsWith("UnityEngine.Application")) continue;
            if (fullName.StartsWith("UnityEngine.Connect") ||
                fullName.StartsWith("UnityEngine.Advertisements") ||
                fullName.StartsWith("UnityEngine.Rendering") ||
                fullName.StartsWith("UnityEngine.QualitySettings")
               ) continue;

            logger.LogDebug($"Saving extern property: {fullName}");
            LoggingUtils.DiskLogger.Flush();

            var value = patchReverseInvoker.Invoke(() => get.Invoke(null, []));
            _externMethodSaves.Add((set, DeepCopy.MakeDeepCopy(value), fullName));
        }
    }

    private readonly string[] _skipProperties =
    [
        "UnityEngine.AssetBundleLoadingCache.maxBlocksPerFile", // this crashes the game
        "UnityEngine.DynamicGI.indirectScale", // unity error: Unable to set Indirect Scale. Please set up a new Lighting Settings asset in the Lighting Settings Window.
        "System.*", // probably bad idea
        "UnityEngine.Time.captureDeltaTime", // this is handled by unitas
        "UnityEngine.Time.captureFramerate", // this too
        // profilers are not supported
        "UnityEngine.Profiling.Profiler.logFile",
        "UnityEngine.Profiling.Profiler.enableBinaryLog",
        "UnityEngine.Profiling.Profiler.enabled"
    ];

    private readonly string[] _knownProperties =
    [
        "UnityEngine.Application.isLoadingLevel",
        "UnityEngine.Application.isPlaying",
        "UnityEngine.Application.isFocused",
        "UnityEngine.Application.buildGUID",
        "UnityEngine.Application.runInBackground",
        "UnityEngine.Application.isBatchMode",
        "UnityEngine.Application.isTestRun",
        "UnityEngine.Application.isHumanControllingUs",
        "UnityEngine.Application.dataPath",
        "UnityEngine.Application.streamingAssetsPath",
        "UnityEngine.Application.persistentDataPath",
        "UnityEngine.Application.temporaryCachePath",
        "UnityEngine.Application.absoluteURL",
        "UnityEngine.Application.unityVersion",
        "UnityEngine.Application.unityVersionVer",
        "UnityEngine.Application.unityVersionMaj",
        "UnityEngine.Application.unityVersionMin",
        "UnityEngine.Application.version",
        "UnityEngine.Application.installerName",
        "UnityEngine.Application.identifier",
        "UnityEngine.Application.installMode",
        "UnityEngine.Application.sandboxType",
        "UnityEngine.Application.productName",
        "UnityEngine.Application.companyName",
        "UnityEngine.Application.cloudProjectId",
        "UnityEngine.Application.targetFrameRate",
        "UnityEngine.Application.stackTraceLogType",
        "UnityEngine.Application.consoleLogPath",
        "UnityEngine.Application.backgroundLoadingPriority",
        "UnityEngine.Application.genuine",
        "UnityEngine.Application.genuineCheckAvailable",
        "UnityEngine.Application.submitAnalytics",
        "UnityEngine.Application.platform",
        "UnityEngine.Application.systemLanguage",
        "UnityEngine.Application.internetReachability",
        "UnityEngine.Application.isLoadingLevel",
        "UnityEngine.Application.isPlaying",
        "UnityEngine.Application.isFocused",
        "UnityEngine.Application.buildGUID",
        "UnityEngine.Application.runInBackground",
        "UnityEngine.Application.isBatchMode",
        "UnityEngine.Application.isTestRun",
        "UnityEngine.Application.isHumanControllingUs",
        "UnityEngine.Application.dataPath",
        "UnityEngine.Application.streamingAssetsPath",
        "UnityEngine.Application.persistentDataPath",
        "UnityEngine.Application.temporaryCachePath",
        "UnityEngine.Application.absoluteURL",
        "UnityEngine.Application.unityVersion",
        "UnityEngine.Application.unityVersionVer",
        "UnityEngine.Application.unityVersionMaj",
        "UnityEngine.Application.unityVersionMin",
        "UnityEngine.Application.version",
        "UnityEngine.Application.installerName",
        "UnityEngine.Application.identifier",
        "UnityEngine.Application.installMode",
        "UnityEngine.Application.sandboxType",
        "UnityEngine.Application.productName",
        "UnityEngine.Application.companyName",
        "UnityEngine.Application.cloudProjectId",
        "UnityEngine.Application.targetFrameRate",
        "UnityEngine.Application.stackTraceLogType",
        "UnityEngine.Application.consoleLogPath",
        "UnityEngine.Application.backgroundLoadingPriority",
        "UnityEngine.Application.genuine",
        "UnityEngine.Application.genuineCheckAvailable",
        "UnityEngine.Application.submitAnalytics",
        "UnityEngine.Application.platform",
        "UnityEngine.Application.systemLanguage",
        "UnityEngine.Application.internetReachability",
        "UnityEngine.Time.time",
        "UnityEngine.Time.timeAsDouble",
        "UnityEngine.Time.timeSinceLevelLoad",
        "UnityEngine.Time.timeSinceLevelLoadAsDouble",
        "UnityEngine.Time.deltaTime",
        "UnityEngine.Time.fixedTime",
        "UnityEngine.Time.fixedTimeAsDouble",
        "UnityEngine.Time.unscaledTime",
        "UnityEngine.Time.unscaledTimeAsDouble",
        "UnityEngine.Time.fixedUnscaledTime",
        "UnityEngine.Time.fixedUnscaledTimeAsDouble",
        "UnityEngine.Time.unscaledDeltaTime",
        "UnityEngine.Time.fixedUnscaledDeltaTime",
        "UnityEngine.Time.fixedDeltaTime",
        "UnityEngine.Time.maximumDeltaTime",
        "UnityEngine.Time.smoothDeltaTime",
        "UnityEngine.Time.maximumParticleDeltaTime",
        "UnityEngine.Time.timeScale",
        "UnityEngine.Time.frameCount",
        "UnityEngine.Time.renderedFrameCount",
        "UnityEngine.Time.realtimeSinceStartup",
        "UnityEngine.Time.realtimeSinceStartupAsDouble",
        "UnityEngine.Time.captureDeltaTime",
        "UnityEngine.Time.inFixedTimeStep",
        "UnityEngine.QualitySettings.pixelLightCount",
        "UnityEngine.QualitySettings.shadows",
        "UnityEngine.QualitySettings.shadowProjection",
        "UnityEngine.QualitySettings.shadowCascades",
        "UnityEngine.QualitySettings.shadowDistance",
        "UnityEngine.QualitySettings.shadowResolution",
        "UnityEngine.QualitySettings.shadowmaskMode",
        "UnityEngine.QualitySettings.shadowNearPlaneOffset",
        "UnityEngine.QualitySettings.shadowCascade2Split",
        "UnityEngine.QualitySettings.lodBias",
        "UnityEngine.QualitySettings.anisotropicFiltering",
        "UnityEngine.QualitySettings.masterTextureLimit",
        "UnityEngine.QualitySettings.globalTextureMipmapLimit",
        "UnityEngine.QualitySettings.maximumLODLevel",
        "UnityEngine.QualitySettings.enableLODCrossFade",
        "UnityEngine.QualitySettings.particleRaycastBudget",
        "UnityEngine.QualitySettings.softParticles",
        "UnityEngine.QualitySettings.softVegetation",
        "UnityEngine.QualitySettings.vSyncCount",
        "UnityEngine.QualitySettings.realtimeGICPUUsage",
        "UnityEngine.QualitySettings.antiAliasing",
        "UnityEngine.QualitySettings.asyncUploadTimeSlice",
        "UnityEngine.QualitySettings.asyncUploadBufferSize",
        "UnityEngine.QualitySettings.asyncUploadPersistentBuffer",
        "UnityEngine.QualitySettings.realtimeReflectionProbes",
        "UnityEngine.QualitySettings.billboardsFaceCameraPosition",
        "UnityEngine.QualitySettings.useLegacyDetailDistribution",
        "UnityEngine.QualitySettings.resolutionScalingFixedDPIFactor",
        "UnityEngine.QualitySettings.terrainQualityOverrides",
        "UnityEngine.QualitySettings.terrainPixelError",
        "UnityEngine.QualitySettings.terrainDetailDensityScale",
        "UnityEngine.QualitySettings.terrainBasemapDistance",
        "UnityEngine.QualitySettings.terrainDetailDistance",
        "UnityEngine.QualitySettings.terrainTreeDistance",
        "UnityEngine.QualitySettings.terrainBillboardStart",
        "UnityEngine.QualitySettings.terrainFadeLength",
        "UnityEngine.QualitySettings.terrainMaxTrees",
        "UnityEngine.QualitySettings.INTERNAL_renderPipeline",
        "UnityEngine.QualitySettings.blendWeights",
        "UnityEngine.QualitySettings.skinWeights",
        "UnityEngine.QualitySettings.count",
        "UnityEngine.QualitySettings.streamingMipmapsActive",
        "UnityEngine.QualitySettings.streamingMipmapsMemoryBudget",
        "UnityEngine.QualitySettings.streamingMipmapsRenderersPerFrame",
        "UnityEngine.QualitySettings.streamingMipmapsMaxLevelReduction",
        "UnityEngine.QualitySettings.streamingMipmapsAddAllCameras",
        "UnityEngine.QualitySettings.streamingMipmapsMaxFileIORequests",
        "UnityEngine.QualitySettings.maxQueuedFrames",
        "UnityEngine.QualitySettings.names",
        "UnityEngine.QualitySettings.desiredColorSpace",
        "UnityEngine.QualitySettings.activeColorSpace",
    ];
}