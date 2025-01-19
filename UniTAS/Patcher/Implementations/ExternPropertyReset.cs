using System;
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
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using MethodImplAttributes = Mono.Cecil.MethodImplAttributes;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ForceInstantiate]
public class ExternPropertyReset
{
    private readonly List<(MethodBase, object, string)> _externMethodSaves = [];

    private readonly ILogger _logger;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IUpdateEvents _updateEvents;

    public ExternPropertyReset(ILogger logger,
        IPatchReverseInvoker patchReverseInvoker,
        IGameRestart gameRestart,
        IUpdateEvents updateEvents)
    {
        _logger = logger;
        _patchReverseInvoker = patchReverseInvoker;
        _updateEvents = updateEvents;
        updateEvents.OnAwakeUnconditional += AwakeUnconditional;
        gameRestart.OnPreGameRestart += OnPreGameRestart;
    }

    private void OnPreGameRestart()
    {
        foreach (var (setMethod, value, name) in _externMethodSaves)
        {
            _logger.LogDebug($"Resetting extern property: {name}");
            LoggingUtils.DiskLogger.Flush();
            var valueClone = DeepCopy.MakeDeepCopy(value);
            try
            {
                _patchReverseInvoker.Invoke((setter, cloned) => setter.Invoke(null, [cloned]), setMethod, valueClone);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to set value for extern property: {name}, exception: {e}");
            }
        }
    }

    private void AwakeUnconditional()
    {
        _updateEvents.OnAwakeUnconditional -= AwakeUnconditional;

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

            if (_skipProperties.Contains(fullName) || _skipProperties.Any(x => fullName.Like(x))) continue;
            if (GameInfoManual.NoGraphics && _skipPropertiesNoGraphics.Contains(fullName) ||
                _skipPropertiesNoGraphics.Any(x => fullName.Like(x))) continue;

            var get = getMethod?.ResolveReflection();
            var set = setMethod?.ResolveReflection();

            if (!_knownProperties.Contains(fullName))
            {
                _logger.LogWarning(
                    $"Found unknown extern property: {fullName}, get exists: {get != null}, set exists: {set != null}");
            }

            // right now, we only support extern properties that have both get and set
            // this could be included explicitly in the future with a list, but this is fine until set needs to be handled
            if (get == null || set == null) continue;

            _logger.LogDebug($"Saving extern property: {fullName}");
            LoggingUtils.DiskLogger.Flush();

            object value;
            try
            {
                value = _patchReverseInvoker.Invoke(getter => getter.Invoke(null, []), get);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to get value for extern property: {fullName}, exception: {e}");
                continue;
            }

            _externMethodSaves.Add((set, DeepCopy.MakeDeepCopy(value), fullName));
        }
    }

    // for when there is no game ui
    private readonly HashSet<string> _skipPropertiesNoGraphics =
    [
        "UnityEngine.Graphics.activeTier", "UnityEngine.GL.wireframe", "UnityEngine.GL.sRGBWrite",
        "UnityEngine.GL.invertCulling", "UnityEngine.QualitySettings.maxQueuedFrames",
        "UnityEngine.Texture.allowThreadedTextureCreation",
        "UnityEngine.Rendering.LoadSt,eActionDebugModeSettings.LoadSt,eDebugModeEnabled",
        "UnityEngine.Experimental.Rendering.GraphicsDeviceSettings.waitF,PresentSyncPoint",
        "UnityEngine.Experimental.Rendering.GraphicsDeviceSettings.graphicsJobsSyncPoint",

        "UnityEngine.Rendering.LoadStoreActionDebugModeSettings.LoadStoreDebugModeEnabled",
        "UnityEngine.Experimental.Rendering.GraphicsDeviceSettings.waitForPresentSyncPoint"
    ];

    private readonly HashSet<string> _skipProperties =
    [
        // crashes game
        "UnityEngine.AssetBundleLoadingCache.maxBlocksPerFile",
        "UnityEngine.Advertisements.UnityAdsManager.enabled",
        "UnityEngine.Advertisements.UnityAdsManager.initializeOnStartup",
        "UnityEngine.Advertisements.UnityAdsManager.testMode",
        "UnityEngine.Advertisements.UnityAdsSettings.enabled",
        "UnityEngine.Advertisements.UnityAdsSettings.initializeOnStartup",
        "UnityEngine.Advertisements.UnityAdsSettings.testMode",
        "UnityEngine.DynamicGI.indirectScale", // unity error: Unable to set Indirect Scale. Please set up a new Lighting Settings asset in the Lighting Settings Window.
        "System.*", // probably bad idea
        // handled by unitas
        "UnityEngine.Time.captureDeltaTime", "UnityEngine.Time.captureFramerate",
        "UnityEngine.Screen.fullScreen", "UnityEngine.Screen.fullScreenMode",
        // profilers are not supported
        "UnityEngine.Profiling.Profiler.logFile",
        "UnityEngine.Profiling.Profiler.enableBinaryLog",
        "UnityEngine.Profiling.Profiler.enabled",
        // error: Virtual texturing is not enabled in the player settings
        // might not matter, but I'm putting it here for now lmao
        "UnityEngine.Rendering.VirtualTexturing.Debugging.debugTilesEnabled",
        "UnityEngine.Rendering.VirtualTexturing.Debugging.resolvingEnabled",
        "UnityEngine.Rendering.VirtualTexturing.Debugging.flushEveryTickEnabled",
        // stops the game from starting
        "UnityEngine.CrashReportHandler.CrashReportHandler.*",
        // no point in these
        "UnityEngine.Analytics.*",
        "UnityEngine.Connect.UnityConnectSettings.*",
        // clipboard access
        "UnityEngine.GUIUtility.systemCopyBuffer",
        // new input system stuff
        "UnityEngineInternal.Input.NativeInputSystem.*",
        // null exception
        "UnityEngine.Rendering.GraphicsSettings.INTERNAL_defaultRenderPipeline",
        "UnityEngine.QualitySettings.INTERNAL_renderPipeline",
        // readonly property (?)
        "UnityEngine.Caching.enabled",
        // missing method exception, also pointless
        "UnityEngine.AndroidJNIHelper.debug",
        // strange test properties
        "UnityEngine.ExceptionTests.PropertyThatCanThrow",
        "UnityEngine.ExceptionTests.PropertyGetThatCanThrow",
        "UnityEngine.ExceptionTests.PropertySetThatCanThrow"
    ];

    private readonly HashSet<string> _knownProperties =
    [
        "UnityEngine.Application.isLoadingLevel", "UnityEngine.Application.isPlaying",
        "UnityEngine.Application.isFocused", "UnityEngine.Application.buildGUID",
        "UnityEngine.Application.runInBackground", "UnityEngine.Application.isBatchMode",
        "UnityEngine.Application.isTestRun", "UnityEngine.Application.isHumanControllingUs",
        "UnityEngine.Application.dataPath", "UnityEngine.Application.streamingAssetsPath",
        "UnityEngine.Application.persistentDataPath", "UnityEngine.Application.temporaryCachePath",
        "UnityEngine.Application.absoluteURL", "UnityEngine.Application.unityVersion",
        "UnityEngine.Application.unityVersionVer", "UnityEngine.Application.unityVersionMaj",
        "UnityEngine.Application.unityVersionMin", "UnityEngine.Application.version",
        "UnityEngine.Application.installerName", "UnityEngine.Application.identifier",
        "UnityEngine.Application.installMode", "UnityEngine.Application.sandboxType",
        "UnityEngine.Application.productName", "UnityEngine.Application.companyName",
        "UnityEngine.Application.cloudProjectId", "UnityEngine.Application.targetFrameRate",
        "UnityEngine.Application.stackTraceLogType", "UnityEngine.Application.consoleLogPath",
        "UnityEngine.Application.backgroundLoadingPriority", "UnityEngine.Application.genuine",
        "UnityEngine.Application.genuineCheckAvailable", "UnityEngine.Application.submitAnalytics",
        "UnityEngine.Application.platform", "UnityEngine.Application.systemLanguage",
        "UnityEngine.Application.internetReachability", "UnityEngine.Application.isLoadingLevel",
        "UnityEngine.Application.isPlaying", "UnityEngine.Application.isFocused", "UnityEngine.Application.buildGUID",
        "UnityEngine.Application.runInBackground", "UnityEngine.Application.isBatchMode",
        "UnityEngine.Application.isTestRun", "UnityEngine.Application.isHumanControllingUs",
        "UnityEngine.Application.dataPath", "UnityEngine.Application.streamingAssetsPath",
        "UnityEngine.Application.persistentDataPath", "UnityEngine.Application.temporaryCachePath",
        "UnityEngine.Application.absoluteURL", "UnityEngine.Application.unityVersion",
        "UnityEngine.Application.unityVersionVer", "UnityEngine.Application.unityVersionMaj",
        "UnityEngine.Application.unityVersionMin", "UnityEngine.Application.version",
        "UnityEngine.Application.installerName", "UnityEngine.Application.identifier",
        "UnityEngine.Application.installMode", "UnityEngine.Application.sandboxType",
        "UnityEngine.Application.productName", "UnityEngine.Application.companyName",
        "UnityEngine.Application.cloudProjectId", "UnityEngine.Application.targetFrameRate",
        "UnityEngine.Application.stackTraceLogType", "UnityEngine.Application.consoleLogPath",
        "UnityEngine.Application.backgroundLoadingPriority", "UnityEngine.Application.genuine",
        "UnityEngine.Application.genuineCheckAvailable", "UnityEngine.Application.submitAnalytics",
        "UnityEngine.Application.platform", "UnityEngine.Application.systemLanguage",
        "UnityEngine.Application.internetReachability", "UnityEngine.Time.time", "UnityEngine.Time.timeAsDouble",
        "UnityEngine.Time.timeSinceLevelLoad", "UnityEngine.Time.timeSinceLevelLoadAsDouble",
        "UnityEngine.Time.deltaTime", "UnityEngine.Time.fixedTime", "UnityEngine.Time.fixedTimeAsDouble",
        "UnityEngine.Time.unscaledTime", "UnityEngine.Time.unscaledTimeAsDouble", "UnityEngine.Time.fixedUnscaledTime",
        "UnityEngine.Time.fixedUnscaledTimeAsDouble", "UnityEngine.Time.unscaledDeltaTime",
        "UnityEngine.Time.fixedUnscaledDeltaTime", "UnityEngine.Time.fixedDeltaTime",
        "UnityEngine.Time.maximumDeltaTime", "UnityEngine.Time.smoothDeltaTime",
        "UnityEngine.Time.maximumParticleDeltaTime", "UnityEngine.Time.timeScale", "UnityEngine.Time.frameCount",
        "UnityEngine.Time.renderedFrameCount", "UnityEngine.Time.realtimeSinceStartup",
        "UnityEngine.Time.realtimeSinceStartupAsDouble", "UnityEngine.Time.inFixedTimeStep",
        "UnityEngine.QualitySettings.pixelLightCount", "UnityEngine.QualitySettings.shadows",
        "UnityEngine.QualitySettings.shadowProjection", "UnityEngine.QualitySettings.shadowCascades",
        "UnityEngine.QualitySettings.shadowDistance", "UnityEngine.QualitySettings.shadowResolution",
        "UnityEngine.QualitySettings.shadowmaskMode", "UnityEngine.QualitySettings.shadowNearPlaneOffset",
        "UnityEngine.QualitySettings.shadowCascade2Split", "UnityEngine.QualitySettings.lodBias",
        "UnityEngine.QualitySettings.anisotropicFiltering", "UnityEngine.QualitySettings.masterTextureLimit",
        "UnityEngine.QualitySettings.globalTextureMipmapLimit", "UnityEngine.QualitySettings.maximumLODLevel",
        "UnityEngine.QualitySettings.enableLODCrossFade", "UnityEngine.QualitySettings.particleRaycastBudget",
        "UnityEngine.QualitySettings.softParticles", "UnityEngine.QualitySettings.softVegetation",
        "UnityEngine.QualitySettings.vSyncCount", "UnityEngine.QualitySettings.realtimeGICPUUsage",
        "UnityEngine.QualitySettings.antiAliasing", "UnityEngine.QualitySettings.asyncUploadTimeSlice",
        "UnityEngine.QualitySettings.asyncUploadBufferSize", "UnityEngine.QualitySettings.asyncUploadPersistentBuffer",
        "UnityEngine.QualitySettings.realtimeReflectionProbes",
        "UnityEngine.QualitySettings.billboardsFaceCameraPosition",
        "UnityEngine.QualitySettings.useLegacyDetailDistribution",
        "UnityEngine.QualitySettings.resolutionScalingFixedDPIFactor",
        "UnityEngine.QualitySettings.terrainQualityOverrides", "UnityEngine.QualitySettings.terrainPixelError",
        "UnityEngine.QualitySettings.terrainDetailDensityScale", "UnityEngine.QualitySettings.terrainBasemapDistance",
        "UnityEngine.QualitySettings.terrainDetailDistance", "UnityEngine.QualitySettings.terrainTreeDistance",
        "UnityEngine.QualitySettings.terrainBillboardStart", "UnityEngine.QualitySettings.terrainFadeLength",
        "UnityEngine.QualitySettings.terrainMaxTrees",
        "UnityEngine.QualitySettings.blendWeights", "UnityEngine.QualitySettings.skinWeights",
        "UnityEngine.QualitySettings.count", "UnityEngine.QualitySettings.streamingMipmapsActive",
        "UnityEngine.QualitySettings.streamingMipmapsMemoryBudget",
        "UnityEngine.QualitySettings.streamingMipmapsRenderersPerFrame",
        "UnityEngine.QualitySettings.streamingMipmapsMaxLevelReduction",
        "UnityEngine.QualitySettings.streamingMipmapsAddAllCameras",
        "UnityEngine.QualitySettings.streamingMipmapsMaxFileIORequests", "UnityEngine.QualitySettings.maxQueuedFrames",
        "UnityEngine.QualitySettings.names", "UnityEngine.QualitySettings.desiredColorSpace",
        "UnityEngine.QualitySettings.activeColorSpace", "UnityEngine.RenderSettings.fog",
        "UnityEngine.RenderSettings.fogStartDistance", "UnityEngine.RenderSettings.fogEndDistance",
        "UnityEngine.RenderSettings.fogMode", "UnityEngine.RenderSettings.fogDensity",
        "UnityEngine.RenderSettings.ambientMode", "UnityEngine.RenderSettings.ambientIntensity",
        "UnityEngine.RenderSettings.skybox", "UnityEngine.RenderSettings.sun",
        "UnityEngine.RenderSettings.customReflectionTexture", "UnityEngine.RenderSettings.reflectionIntensity",
        "UnityEngine.RenderSettings.reflectionBounces", "UnityEngine.RenderSettings.defaultReflection",
        "UnityEngine.RenderSettings.defaultReflectionMode", "UnityEngine.RenderSettings.defaultReflectionResolution",
        "UnityEngine.RenderSettings.haloStrength", "UnityEngine.RenderSettings.flareStrength",
        "UnityEngine.RenderSettings.flareFadeSpeed", "UnityEngine.AI.NavMesh.avoidancePredictionTime",
        "UnityEngine.AI.NavMesh.pathfindingIterationsPerFrame",
        "UnityEngine.HumanTrait.MuscleCount", "UnityEngine.HumanTrait.MuscleName", "UnityEngine.HumanTrait.BoneCount",
        "UnityEngine.HumanTrait.BoneName", "UnityEngine.HumanTrait.RequiredBoneCount",
        "UnityEngine.AssetBundleLoadingCache.blockCount", "UnityEngine.AssetBundleLoadingCache.blockSize",
        "UnityEngine.AudioSettings.driverCapabilities", "UnityEngine.AudioSettings.profilerCaptureFlags",
        "UnityEngine.AudioSettings.dspTime", "UnityEngine.AudioSettings.unityAudioDisabled",
        "UnityEngine.AudioListener.volume", "UnityEngine.AudioListener.pause", "UnityEngine.Microphone.devices",
        "UnityEngine.Microphone.isAnyDeviceRecording", "UnityEngine.WebCamTexture.devices",
        "UnityEngine.ClusterNetwork.isMasterOfCluster", "UnityEngine.ClusterNetwork.isDisconnected",
        "UnityEngine.ClusterNetwork.nodeIndex", "Unity.Loading.ContentLoadInterface.LoadingPriority",
        "UnityEngineInternal.GIDebugVisualisation.cycleMode", "UnityEngineInternal.GIDebugVisualisation.pauseCycleMode",
        "UnityEngineInternal.GIDebugVisualisation.texType", "Unity.Jobs.LowLevel.Unsafe.JobsUtility.IsExecutingJob",
        "Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobDebuggerEnabled",
        "Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobCompilerEnabled",
        "Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount",
        "Unity.Jobs.LowLevel.Unsafe.JobsUtility.ThreadIndex", "Unity.Jobs.LowLevel.Unsafe.JobsUtility.ThreadIndexCount",
        "Unity.Profiling.LowLevel.Unsafe.ProfilerUnsafeUtility.Timestamp",
        "Unity.Burst.LowLevel.BurstCompilerService.IsInitialized", "UnityEngine.Caching.compressionEnabled",
        "UnityEngine.Caching.ready", "UnityEngine.Caching.spaceOccupied", "UnityEngine.Caching.spaceFree",
        "UnityEngine.Caching.maximumAvailableDiskSpace", "UnityEngine.Caching.expirationDelay",
        "UnityEngine.Caching.cacheCount", "UnityEngine.Camera.main", "UnityEngine.Camera.current",
        "UnityEngine.ReflectionProbe.minBakedCubemapResolution",
        "UnityEngine.ReflectionProbe.maxBakedCubemapResolution", "UnityEngine.ReflectionProbe.defaultTexture",
        "UnityEngine.Debug.developerConsoleEnabled", "UnityEngine.Debug.developerConsoleVisible",
        "UnityEngine.Debug.isDebugBuild", "UnityEngine.Debug.diagnosticSwitches",
        "UnityEngine.DynamicGI.updateThreshold", "UnityEngine.DynamicGI.materialUpdateTimeSlice",
        "UnityEngine.DynamicGI.synchronousMode", "UnityEngine.DynamicGI.isConverged",
        "UnityEngine.DynamicGI.scheduledMaterialUpdatesCount", "UnityEngine.DynamicGI.asyncMaterialUpdates",
        "UnityEngine.Gizmos.exposure", "UnityEngine.Gizmos.probeSize", "UnityEngine.Screen.width",
        "UnityEngine.Screen.height", "UnityEngine.Screen.dpi", "UnityEngine.Screen.sleepTimeout",
        "UnityEngine.Screen.cutouts",
        "UnityEngine.Screen.resolutions", "UnityEngine.Screen.brightness", "UnityEngine.Graphics.activeTier",
        "UnityEngine.GL.wireframe", "UnityEngine.GL.sRGBWrite", "UnityEngine.GL.invertCulling",
        "UnityEngine.ScalableBufferManager.widthScaleFactor", "UnityEngine.ScalableBufferManager.heightScaleFactor",
        "UnityEngine.LightmapSettings.lightmaps", "UnityEngine.LightmapSettings.lightmapsMode",
        "UnityEngine.LightmapSettings.lightProbes", "UnityEngine.Shader.maximumChunksOverride",
        "UnityEngine.Shader.globalMaximumLOD", "UnityEngine.Shader.globalRenderPipeline",
        "UnityEngine.LightProbeProxyVolume.isFeatureSupported", "UnityEngine.LODGroup.crossFadeAnimationDuration",
        "UnityEngine.Texture.masterTextureLimit", "UnityEngine.Texture.globalMipmapLimit",
        "UnityEngine.Texture.anisotropicFiltering", "UnityEngine.Texture.totalTextureMemory",
        "UnityEngine.Texture.desiredTextureMemory", "UnityEngine.Texture.targetTextureMemory",
        "UnityEngine.Texture.currentTextureMemory", "UnityEngine.Texture.nonStreamingTextureMemory",
        "UnityEngine.Texture.streamingMipmapUploadCount", "UnityEngine.Texture.streamingRendererCount",
        "UnityEngine.Texture.streamingTextureCount", "UnityEngine.Texture.nonStreamingTextureCount",
        "UnityEngine.Texture.streamingTexturePendingLoadCount", "UnityEngine.Texture.streamingTextureLoadingCount",
        "UnityEngine.Texture.streamingTextureForceLoadAll", "UnityEngine.Texture.streamingTextureDiscardUnusedMips",
        "UnityEngine.Texture.allowThreadedTextureCreation", "UnityEngine.Texture2D.whiteTexture",
        "UnityEngine.Texture2D.blackTexture", "UnityEngine.Texture2D.redTexture", "UnityEngine.Texture2D.grayTexture",
        "UnityEngine.Texture2D.linearGrayTexture", "UnityEngine.Texture2D.normalTexture",
        "UnityEngine.Texture2DArray.allSlices", "UnityEngine.Cursor.visible", "UnityEngine.Cursor.lockState",
        "UnityEngine.Random.value", "UnityEngine.Random.seed", "UnityEngine.TouchScreenKeyboard.hideInput",
        "UnityEngine.TouchScreenKeyboard.visible", "UnityEngine.U2D.PixelPerfectRendering.pixelSnapSpacing",
        "UnityEngine.Profiling.Profiler.supported", "UnityEngine.Profiling.Profiler.maxUsedMemory",
        "UnityEngine.Profiling.Profiler.enableAllocationCallstacks", "UnityEngine.Profiling.Profiler.usedHeapSizeLong",
        "UnityEngine.Windows.CrashReporting.crashReportFolder",
        "UnityEngine.Windows.Speech.PhraseRecognitionSystem.isSupported",
        "UnityEngine.Windows.Speech.PhraseRecognitionSystem.Status",
        "UnityEngine.Scripting.GarbageCollector.isIncremental",
        "UnityEngine.Scripting.GarbageCollector.incrementalTimeSliceNanoseconds",
        "UnityEngine.SceneManagement.SceneManager.sceneCount",
        "UnityEngine.SceneManagement.SceneManager.loadedSceneCount", "UnityEngine.TestTools.Coverage.enabled",
        "UnityEngine.Experimental.GlobalIllumination.RenderSettings.useRadianceAmbientProbe",
        "UnityEngine.Experimental.Rendering.GraphicsDeviceSettings.waitForPresentSyncPoint",
        "UnityEngine.Experimental.Rendering.GraphicsDeviceSettings.graphicsJobsSyncPoint",
        "UnityEngine.GUI.changed",
        "UnityEngine.GUI.enabled", "UnityEngine.GUI.depth", "UnityEngine.GUI.usePageScrollbars",
        "UnityEngine.GUI.isInsideList", "UnityEngine.GUI.blendMaterial", "UnityEngine.GUI.blitMaterial",
        "UnityEngine.GUI.roundedRectMaterial", "UnityEngine.GUI.roundedRectWithColorPerBorderMaterial",
        "UnityEngine.GUIClip.enabled", "UnityEngine.GUIDebugger.active", "UnityEngine.GUIUtility.hasModalWindow",
        "UnityEngine.GUIUtility.pixelsPerPoint", "UnityEngine.GUIUtility.guiDepth", "UnityEngine.GUIUtility.mouseUsed",
        "UnityEngine.GUIUtility.textFieldInput", "UnityEngine.GUIUtility.manualTex2SRGBEnabled",
        "UnityEngine.GUIUtility.systemCopyBuffer", "UnityEngine.GUIUtility.compositionString",
        "UnityEngine.GUIUtility.imeCompositionMode", "UnityEngine.Input.simulateMouseWithTouches",
        "UnityEngine.Input.anyKey", "UnityEngine.Input.anyKeyDown", "UnityEngine.Input.inputString",
        "UnityEngine.Input.imeCompositionMode", "UnityEngine.Input.compositionString",
        "UnityEngine.Input.imeIsSelected", "UnityEngine.Input.eatKeyPressOnTextFieldFocus",
        "UnityEngine.Input.mousePresent", "UnityEngine.Input.penEventCount", "UnityEngine.Input.touchCount",
        "UnityEngine.Input.touchPressureSupported", "UnityEngine.Input.stylusTouchSupported",
        "UnityEngine.Input.touchSupported", "UnityEngine.Input.multiTouchEnabled", "UnityEngine.Input.isGyroAvailable",
        "UnityEngine.Input.deviceOrientation", "UnityEngine.Input.compensateSensors",
        "UnityEngine.Input.accelerationEventCount", "UnityEngine.Input.backButtonLeavesApp",
        "UnityEngineInternal.Input.NativeInputSystem.hasDeviceDiscoveredCallback",
        "UnityEngineInternal.Input.NativeInputSystem.currentTime",
        "UnityEngineInternal.Input.NativeInputSystem.currentTimeOffsetToRealtimeSinceStartup",
        "UnityEngineInternal.Input.NativeInputSystem.allowInputDeviceCreationFromEvents",
        "UnityEngine.Physics2D.velocityIterations", "UnityEngine.Physics2D.positionIterations",
        "UnityEngine.Physics2D.queriesHitTriggers", "UnityEngine.Physics2D.queriesStartInColliders",
        "UnityEngine.Physics2D.callbacksOnDisable", "UnityEngine.Physics2D.reuseCollisionCallbacks",
        "UnityEngine.Physics2D.autoSyncTransforms", "UnityEngine.Physics2D.simulationMode",
        "UnityEngine.Physics2D.velocityThreshold", "UnityEngine.Physics2D.maxLinearCorrection",
        "UnityEngine.Physics2D.maxAngularCorrection", "UnityEngine.Physics2D.maxTranslationSpeed",
        "UnityEngine.Physics2D.maxRotationSpeed", "UnityEngine.Physics2D.defaultContactOffset",
        "UnityEngine.Physics2D.baumgarteScale", "UnityEngine.Physics2D.baumgarteTOIScale",
        "UnityEngine.Physics2D.timeToSleep", "UnityEngine.Physics2D.linearSleepTolerance",
        "UnityEngine.Physics2D.angularSleepTolerance", "UnityEngine.Physics.defaultContactOffset",
        "UnityEngine.Physics.sleepThreshold", "UnityEngine.Physics.queriesHitTriggers",
        "UnityEngine.Physics.queriesHitBackfaces", "UnityEngine.Physics.bounceThreshold",
        "UnityEngine.Physics.defaultMaxDepenetrationVelocity", "UnityEngine.Physics.defaultSolverIterations",
        "UnityEngine.Physics.defaultSolverVelocityIterations", "UnityEngine.Physics.simulationMode",
        "UnityEngine.Physics.defaultMaxAngularSpeed", "UnityEngine.Physics.improvedPatchFriction",
        "UnityEngine.Physics.invokeCollisionCallbacks", "UnityEngine.Physics.autoSyncTransforms",
        "UnityEngine.Physics.reuseCollisionCallbacks", "UnityEngine.Physics.interCollisionDistance",
        "UnityEngine.Physics.interCollisionStiffness", "UnityEngine.Physics.interCollisionSettingsToggle",
        "UnityEngine.Terrain.heightmapFormat", "UnityEngine.Terrain.normalmapFormat", "UnityEngine.Terrain.holesFormat",
        "UnityEngine.Terrain.compressedHolesFormat", "UnityEngine.Terrain.activeTerrain",
        "UnityEngine.Terrain.activeTerrains", "UnityEngine.TextCore.LowLevel.FontEngine.isProcessingDone",
        "UnityEngine.TextCore.LowLevel.FontEngine.generationProgress",
        "UnityEngine.VFX.VFXManager.runtimeResources",
        "UnityEngine.VFX.VFXManager.fixedTimeStep", "UnityEngine.VFX.VFXManager.maxDeltaTime",
        "UnityEngine.VFX.VFXManager.maxScrubTime", "UnityEngine.VFX.VFXManager.renderPipeSettingsPath",
        "UnityEngine.VFX.VFXManager.batchEmptyLifetime", "UnityEngine.XR.XRSettings.enabled",
        "UnityEngine.XR.XRSettings.gameViewRenderMode", "UnityEngine.XR.XRSettings.isDeviceActive",
        "UnityEngine.XR.XRSettings.showDeviceView", "UnityEngine.XR.XRSettings.eyeTextureResolutionScale",
        "UnityEngine.XR.XRSettings.eyeTextureWidth", "UnityEngine.XR.XRSettings.eyeTextureHeight",
        "UnityEngine.XR.XRSettings.deviceEyeTextureDimension", "UnityEngine.XR.XRSettings.renderViewportScaleInternal",
        "UnityEngine.XR.XRSettings.occlusionMaskScale", "UnityEngine.XR.XRSettings.useOcclusionMesh",
        "UnityEngine.XR.XRSettings.loadedDeviceName", "UnityEngine.XR.XRSettings.supportedDevices",
        "UnityEngine.XR.XRSettings.stereoRenderingMode", "UnityEngine.XR.XRDevice.refreshRate",
        "UnityEngine.XR.XRDevice.fovZoomFactor", "UnityEngine.Video.VideoPlayer.controlledAudioTrackMaxCount",
        "UnityEngine.XR.InputTracking.disablePositionalTracking",
        "UnityEngine.Rendering.LoadStoreActionDebugModeSettings.LoadStoreDebugModeEnabled",
        "UnityEngine.Rendering.GraphicsSettings.transparencySortMode",
        "UnityEngine.Rendering.GraphicsSettings.realtimeDirectRectangularAreaLights",
        "UnityEngine.Rendering.GraphicsSettings.lightsUseLinearIntensity",
        "UnityEngine.Rendering.GraphicsSettings.lightsUseColorTemperature",
        "UnityEngine.Rendering.GraphicsSettings.defaultRenderingLayerMask",
        "UnityEngine.Rendering.GraphicsSettings.useScriptableRenderPipelineBatching",
        "UnityEngine.Rendering.GraphicsSettings.logWhenShaderIsCompiled",
        "UnityEngine.Rendering.GraphicsSettings.disableBuiltinCustomRenderTextureUpdate",
        "UnityEngine.Rendering.GraphicsSettings.videoShadersIncludeMode",
        "UnityEngine.Rendering.GraphicsSettings.lightProbeOutsideHullStrategy",
        "UnityEngine.Rendering.GraphicsSettings.INTERNAL_currentRenderPipeline",
        "UnityEngine.Rendering.GraphicsSettings.INTERNAL_defaultRenderPipeline",
        "UnityEngine.Rendering.GraphicsSettings.cameraRelativeLightCulling",
        "UnityEngine.Rendering.GraphicsSettings.cameraRelativeShadowCulling",
        "UnityEngine.Rendering.SplashScreen.isFinished",
        "UnityEngine.Rendering.SortingGroup.invalidSortingGroupID",
        "UnityEngine.Rendering.VirtualTexturing.System.enabled",
        "UnityEngine.Rendering.VirtualTexturing.EditorHelpers.tileSize",
        "UnityEngine.Rendering.VirtualTexturing.Debugging.mipPreloadedTextureCount",
        "UnityEngine.Camera.PreviewCullingLayer", "UnityEngine.RenderSettings.customReflection",
        "UnityEngine.Windows.WebCam.WebCam.Mode", "UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings",
        "UnityEngine.Physics2D.autoSimulation", "UnityEngine.Physics2D.alwaysShowColliders",
        "UnityEngine.Physics2D.showColliderSleep", "UnityEngine.Physics2D.showColliderContacts",
        "UnityEngine.Physics2D.showColliderAABB", "UnityEngine.Physics2D.contactArrowScale",
        "UnityEngine.Physics.autoSimulation", "UnityEngine.Experimental.XR.Boundary.visible",
        "UnityEngine.Experimental.XR.Boundary.configured", "UnityEngine.XR.XRSettings.renderScale",
        "UnityEngine.XR.XRDevice.isPresent", "UnityEngine.XR.XRDevice.userPresence", "UnityEngine.XR.XRDevice.family",
        "UnityEngine.XR.XRDevice.model", "UnityEngine.XR.XRDevice.trackingOriginMode",
        "UnityEngine.XR.WSA.HolographicSettings.IsContentProtectionEnabled",
        "UnityEngine.XR.Tango.TangoDevice.baseCoordinateFrame", "UnityEngine.XR.Tango.TangoDevice.depthCameraRate",
        "UnityEngine.XR.Tango.TangoDevice.synchronizeFramerateWithColorCamera",
        "UnityEngine.XR.Tango.TangoDevice.isServiceConnected", "UnityEngine.XR.Tango.TangoDevice.isServiceAvailable",
        "UnityEngine.AudioSettings.speakerMode", "UnityEngine.AudioSettings.outputSampleRate",
        "UnityEngine.Application.streamedBytes", "UnityEngine.Application.isEditor",
        "UnityEngine.Application.isWebPlayer", "UnityEngine.Application.isBatchmode",
        "UnityEngine.Application.srcValue", "UnityEngine.Application.webSecurityEnabled",
        "UnityEngine.Application.webSecurityHostUrl", "UnityEngine.SystemInfo.batteryLevel",
        "UnityEngine.SystemInfo.batteryStatus", "UnityEngine.SystemInfo.operatingSystem",
        "UnityEngine.SystemInfo.operatingSystemFamily", "UnityEngine.SystemInfo.processorType",
        "UnityEngine.SystemInfo.processorFrequency", "UnityEngine.SystemInfo.processorCount",
        "UnityEngine.SystemInfo.systemMemorySize", "UnityEngine.SystemInfo.graphicsMemorySize",
        "UnityEngine.SystemInfo.graphicsDeviceName", "UnityEngine.SystemInfo.graphicsDeviceVendor",
        "UnityEngine.SystemInfo.graphicsDeviceID", "UnityEngine.SystemInfo.graphicsDeviceVendorID",
        "UnityEngine.SystemInfo.graphicsDeviceType", "UnityEngine.SystemInfo.graphicsUVStartsAtTop",
        "UnityEngine.SystemInfo.graphicsDeviceVersion", "UnityEngine.SystemInfo.graphicsShaderLevel",
        "UnityEngine.SystemInfo.graphicsMultiThreaded", "UnityEngine.SystemInfo.supportsShadows",
        "UnityEngine.SystemInfo.supportsRawShadowDepthSampling", "UnityEngine.SystemInfo.supportsRenderTextures",
        "UnityEngine.SystemInfo.supportsMotionVectors", "UnityEngine.SystemInfo.supportsRenderToCubemap",
        "UnityEngine.SystemInfo.supportsImageEffects", "UnityEngine.SystemInfo.supports3DTextures",
        "UnityEngine.SystemInfo.supports2DArrayTextures", "UnityEngine.SystemInfo.supports3DRenderTextures",
        "UnityEngine.SystemInfo.supportsCubemapArrayTextures", "UnityEngine.SystemInfo.copyTextureSupport",
        "UnityEngine.SystemInfo.supportsComputeShaders", "UnityEngine.SystemInfo.supportsInstancing",
        "UnityEngine.SystemInfo.supportsSparseTextures", "UnityEngine.SystemInfo.supportedRenderTargetCount",
        "UnityEngine.SystemInfo.supportsMultisampledTextures", "UnityEngine.SystemInfo.supportsTextureWrapMirrorOnce",
        "UnityEngine.SystemInfo.usesReversedZBuffer", "UnityEngine.SystemInfo.supportsStencil",
        "UnityEngine.SystemInfo.npotSupport", "UnityEngine.SystemInfo.deviceUniqueIdentifier",
        "UnityEngine.SystemInfo.deviceName", "UnityEngine.SystemInfo.deviceModel",
        "UnityEngine.SystemInfo.supportsAccelerometer", "UnityEngine.SystemInfo.supportsGyroscope",
        "UnityEngine.SystemInfo.supportsLocationService", "UnityEngine.SystemInfo.supportsVibration",
        "UnityEngine.SystemInfo.supportsAudio", "UnityEngine.SystemInfo.deviceType",
        "UnityEngine.SystemInfo.maxTextureSize", "UnityEngine.SystemInfo.maxCubemapSize",
        "UnityEngine.SystemInfo.maxRenderTextureSize", "UnityEngine.SystemInfo.supportsAsyncCompute",
        "UnityEngine.SystemInfo.supportsGPUFence", "UnityEngine.Caching.index", "UnityEngine.Camera.allCameras",
        "UnityEngine.Camera.allCamerasCount", "UnityEngine.Graphics.activeColorGamut", "UnityEngine.Screen.orientation",
        "UnityEngine.Light.pixelLightCount", "UnityEngine.Network.incomingPassword", "UnityEngine.Network.logLevel",
        "UnityEngine.Network.connections", "UnityEngine.Network.isClient", "UnityEngine.Network.isServer",
        "UnityEngine.Network.peerType", "UnityEngine.Network.sendRate", "UnityEngine.Network.isMessageQueueRunning",
        "UnityEngine.Network.minimumAllocatableViewIDs", "UnityEngine.Network.useNat",
        "UnityEngine.Network.natFacilitatorIP", "UnityEngine.Network.natFacilitatorPort",
        "UnityEngine.Network.connectionTesterIP", "UnityEngine.Network.connectionTesterPort",
        "UnityEngine.Network.maxConnections", "UnityEngine.Network.proxyIP", "UnityEngine.Network.proxyPort",
        "UnityEngine.Network.useProxy", "UnityEngine.Network.proxyPassword", "UnityEngine.MasterServer.ipAddress",
        "UnityEngine.MasterServer.port", "UnityEngine.MasterServer.updateRate",
        "UnityEngine.MasterServer.dedicatedServer",
        "UnityEngine.Rendering.GraphicsSettings.INTERNAL_renderPipelineAsset",
        "UnityEngine.ProceduralMaterial.isSupported", "UnityEngine.ProceduralMaterial.substanceProcessorUsage",
        "UnityEngine.RenderTexture.active", "UnityEngine.RenderTexture.enabled",
        "UnityEngine.Profiling.Profiler.maxNumberOfSamplesPerFrame", "UnityEngine.Profiling.Profiler.usedHeapSize",
        "UnityEngineInternal.Input.NativeInputSystem.zeroEventTime", "UnityEngine.Physics2D.changeStopsCallbacks",
        "UnityEngine.Physics.minPenetrationForPenalty", "UnityEngine.Physics.sleepVelocity",
        "UnityEngine.Physics.sleepAngularVelocity", "UnityEngine.Physics.maxAngularVelocity",
        "UnityEngine.Physics.penetrationPenaltyForce", "UnityEngine.Networking.NetworkTransport.IsStarted",
    ];
}