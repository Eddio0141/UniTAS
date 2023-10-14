using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;

namespace UniTAS.Patcher.Utils;

public static class TargetPatcherDlls
{
    public static IEnumerable<string> AllDLLs =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly).Select(Path.GetFileName)
            .ToArray();
}