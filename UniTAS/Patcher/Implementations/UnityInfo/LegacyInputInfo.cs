using System.IO;
using AssetsTools.NET.Extra;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Utils;

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

        var inputManagerAsset = file.GetAssetsOfType(AssetClassID.InputManager)[0];
        var inputManager = manager.GetBaseField(assetsFileInstance, inputManagerAsset);

        var axes = inputManager["m_Axes.Array"];

        foreach (var axis in axes)
        {
            var name = axis["m_Name"].AsString;
        }
    }
}