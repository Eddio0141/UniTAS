using System.IO;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.UnityInfo;

[Singleton]
[ExcludeRegisterIfTesting]
public class AssetsManager : IAssetsManager
{
    public AssetsTools.NET.Extra.AssetsManager Instance { get; }

    public AssetsManager(ILogger logger)
    {
        Instance = new();
        var tpkPath = Path.Combine(UniTASPaths.Resources, "uncompressed.tpk");
        if (!File.Exists(tpkPath))
        {
            logger.LogError($"uncompressed.tpk not found in {tpkPath}, can't grab assets");
        }

        Instance.LoadClassPackage(tpkPath);
    }
}