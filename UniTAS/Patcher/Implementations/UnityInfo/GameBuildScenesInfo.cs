using System.Collections.Generic;
using System.IO;
using AssetsTools.NET.Extra;
using BepInEx;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;

namespace UniTAS.Patcher.Implementations.UnityInfo;

[Singleton]
public class GameBuildScenesInfo : IGameBuildScenesInfo
{
    public GameBuildScenesInfo(IAssetsManager assetsManager, ILogger logger)
    {
        var globalGameManagersPath = Directory.GetParent(Paths.ManagedPath)?.FullName;
        if (globalGameManagersPath == null)
        {
            logger.LogError("Failed to get globalgamemanagers path");
            return;
        }

        globalGameManagersPath = Path.Combine(globalGameManagersPath, "globalgamemanagers");

        if (!File.Exists(globalGameManagersPath))
        {
            logger.LogInfo("Failed to find globalgamemanagers file");
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

        var buildSettingsAssets = file.GetAssetsOfType(AssetClassID.BuildSettings);
        if (buildSettingsAssets.Count == 0)
        {
            logger.LogError("Failed to find build settings asset");
            return;
        }

        var buildSettingsAsset = buildSettingsAssets[0];
        var buildSettings = manager.GetBaseField(assetsFileInstance, buildSettingsAsset);

        var scenes = buildSettings["scenes.Array"];
        var useFullPath = new HashSet<string>();
        var i = 0;
        foreach (var scene in scenes)
        {
            var path = scene.AsString;
            PathToIndex[path] = i;
            var name = Path.GetFileNameWithoutExtension(path);
            if (useFullPath.Contains(name))
            {
                name = path;
            }
            else if (NameToPath.TryGetValue(name, out var fixPath))
            {
                // duplicate name, full path must be used
                useFullPath.Add(name);

                // also fix duplicate, previous entry will also need to use full path
                var fixIndex = PathToIndex[fixPath];

                PathToIndex.Remove(fixPath);
                PathToName.Remove(fixPath);
                NameToPath.Remove(name);

                PathToIndex[fixPath] = fixIndex;
                PathToName[fixPath] = fixPath;
                NameToPath[fixPath] = fixPath;

                name = path;
            }

            PathToIndex[path] = i;
            PathToName[path] = name;
            NameToPath[name] = path;

            i++;
        }
    }

    public Dictionary<string, int> PathToIndex { get; } = new();
    public Dictionary<string, string> PathToName { get; } = new();
    public Dictionary<string, string> NameToPath { get; } = new();
}