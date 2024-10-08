using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using UniTAS.Patcher.Extensions;

namespace UniTAS.Patcher.Utils;

public static class TargetPatcherDlls
{
    private static IEnumerable<string> AllDLLs =>
        Directory.GetFiles(Paths.ManagedPath, "*.dll", SearchOption.TopDirectoryOnly).Select(Path.GetFileName)
            .ToArray();

    // lazy initialization, for some reason directly initializing to the property breaks this
    private static IEnumerable<string> _allExcludedDLLs;

    public static IEnumerable<string> AllExcludedDLLs
    {
        get
        {
            _allExcludedDLLs ??= AllDLLs.Where(x =>
            {
                var fileWithoutExtension = Path.GetFileNameWithoutExtension(x);
                return fileWithoutExtension == null ||
                       // AssemblyIncludeRaw.Any(a => fileWithoutExtension.Like(a)) ||
                       !AssemblyExclusionsRaw.Any(a => fileWithoutExtension.Like(a));
            });
            return _allExcludedDLLs;
        }
    }

    private static List<string> AssemblyExclusionsRaw { get; } =
    [
        // c# related
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        "Mono.*",
        "Mono",
        // no need
        "Newtonsoft.Json",

        // should be fine
        "UnityEngine.IMGUIModule",

        "Unity.InputSystem",
        "Unity.InputSystem.ForUI",
        "UnityEngine.InputModule",
        
        // ignore rewired, there's a fix for it
        "Rewired_*"
    ];

    // private static string[] AssemblyIncludeRaw { get; } =
    // [
    // ];
}
