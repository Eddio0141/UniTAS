using System.IO;
using AssetsTools.NET.Extra;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;

namespace UniTAS.Patcher.Implementations.UnityInfo;

[Singleton]
[ForceInstantiate]
public class LegacyInputInfo : ILegacyInputInfo
{
    public LegacyInputInfo(IAssetsManager assetsManager, ILogger logger)
    {
        var globalGameManagersPath = Directory.GetParent(Paths.ManagedPath)?.FullName;
        if (globalGameManagersPath == null)
        {
            logger.LogError("Failed to get globalGameManagers path");
            return;
        }

        globalGameManagersPath = Path.Combine(globalGameManagersPath, "globalgamemanagers");

        if (!File.Exists(globalGameManagersPath))
        {
            logger.LogError("Failed to find globalGameManagers file");
            return;
        }

        var manager = assetsManager.Instance;
        if (manager == null)
        {
            logger.LogError("Failed to get assets manager instance");
            return;
        }

        var assetsFileInstance = manager.LoadAssetsFile(globalGameManagersPath, true);
        var file = assetsFileInstance.file;
        manager.LoadClassDatabaseFromPackage(file.Metadata.UnityVersion);

        var inputManagerAssets = file.GetAssetsOfType(AssetClassID.InputManager);

        if (inputManagerAssets.Count == 0)
        {
            logger.LogError("Failed to find input manager asset");
            return;
        }

        var inputManagerAsset = inputManagerAssets[0];
        var inputManager = manager.GetBaseField(assetsFileInstance, inputManagerAsset);

        var axes = inputManager["m_Axes.Array"];

        foreach (var axis in axes)
        {
            // getting those shouldn't fail, unless the new version of Unity changed the format

            var name = axis["m_Name"].AsString;
            var negativeButton = axis["negativeButton"].AsString;
            var positiveButton = axis["positiveButton"].AsString;
            var altNegativeButton = axis["altNegativeButton"].AsString;
            var altPositiveButton = axis["altPositiveButton"].AsString;
            var gravity = axis["gravity"].AsFloat;
            var dead = axis["dead"].AsFloat;
            var sensitivity = axis["sensitivity"].AsFloat;
            var snap = axis["snap"].AsBool;
            var invert = axis["invert"].AsBool;
            // TODO translate to enum
            var typeRaw = axis["type"].AsInt;
            // TODO translate to enum
            var axisNumRaw = axis["axis"].AsInt;
            // TODO translate to enum
            var joyNumRaw = axis["joyNum"].AsInt;

            logger.LogDebug($"Found legacy input axis: {name}");

            var type = (AxisType)typeRaw;
            var axisNum = (AxisChoice)axisNumRaw;
            var joyNum = (JoyNum)joyNumRaw;

            var legacyInputAxis = new LegacyInputAxis(name, negativeButton, positiveButton, altNegativeButton,
                altPositiveButton, gravity, dead, sensitivity, snap, invert, type, axisNum, joyNum);
        }
    }
}