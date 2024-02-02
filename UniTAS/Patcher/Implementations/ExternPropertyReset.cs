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
                    $"Found unknown extern property: {fullName}, get exists: {get != null}, set exists: {set != null}");
            }

            // right now, we only support extern properties that have both get and set
            // this could be included explicitly in the future with a list, but this is fine until set needs to be handled
            if (get == null || set == null) continue;

            // TODO this needs to be removed before merge
            if (propDef.DeclaringType.Namespace is not ("UnityEngine.Time" or "UnityEngine.Application"))
                continue;

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
    ];
}