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
        var oldRunSha256Path = Path.Combine(UniTASPaths.Cache, "UniTASPatcher.sha256");
        var currentRunSha256Path = Utility.CombinePaths(Paths.PatcherPluginPath, "UniTAS", "UniTAS.Patcher.dll");
        var currentRunSha256Stream = File.OpenRead(currentRunSha256Path);
        using var sha256 = SHA256.Create();
        var currentRunSha256 = sha256.ComputeHash(currentRunSha256Stream);

        if (File.Exists(oldRunSha256Path))
        {
            var oldRunSha256 = File.ReadAllBytes(oldRunSha256Path);
            UniTASChanged = !currentRunSha256.SequenceEqual(oldRunSha256);
        }
        else
        {
            UniTASChanged = true;
        }

        File.WriteAllBytes(oldRunSha256Path, currentRunSha256);
    }

    public static bool UniTASChanged { get; }
}