using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BepInEx;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices;

public static class UniTASSha256Info
{
    static UniTASSha256Info()
    {
        // check if config is invalid first
        foreach (var cfgPath in Directory.GetFiles(Paths.ConfigPath, "*.cfg", SearchOption.AllDirectories))
        {
            var cfgCachedPath = Path.Combine(UniTASPaths.ConfigCache, $"{Path.GetFileName(cfgPath)}.sha256");

            var cfgStream = File.OpenRead(cfgPath);
            using var cfgHash = SHA256.Create();
            var cfgHashBytes = cfgHash.ComputeHash(cfgStream);

            if (!File.Exists(cfgCachedPath))
            {
                // new file, just save and move on
                UniTASInvalidCache = true;

                File.WriteAllBytes(cfgCachedPath, cfgHashBytes);
                continue;
            }

            var cacheHash = File.ReadAllBytes(cfgCachedPath);

            if (!UniTASInvalidCache && !cfgHashBytes.SequenceEqual(cacheHash))
            {
                UniTASInvalidCache = true;
            }

            File.WriteAllBytes(cfgCachedPath, cfgHashBytes);
        }

        var oldRunSha256Path = Path.Combine(UniTASPaths.Cache, "UniTASPatcher.sha256");
        var currentRunSha256Path = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "UniTAS.Patcher.dll");
        var currentRunSha256Stream = File.OpenRead(currentRunSha256Path);
        using var sha256 = SHA256.Create();
        var currentRunSha256 = sha256.ComputeHash(currentRunSha256Stream);

        if (!UniTASInvalidCache)
        {
            if (File.Exists(oldRunSha256Path))
            {
                var oldRunSha256 = File.ReadAllBytes(oldRunSha256Path);
                UniTASInvalidCache = !currentRunSha256.SequenceEqual(oldRunSha256);
            }
            else
            {
                UniTASInvalidCache = true;
            }
        }

        File.WriteAllBytes(oldRunSha256Path, currentRunSha256);
    }

   public static bool UniTASInvalidCache { get; }
    public static bool GameCacheInvalid { get; set; }
}