namespace UniTAS.Patcher.Utils;

public static class StaticCtorPatchTargetInfo
{
    public static string[] AssemblyExclusionsRaw { get; } =
    {
        "UnityEngine.*",
        "UnityEngine",
        "Unity.*",
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        "Mono.*",
        "Mono",
        "MonoMod.*",
        "BepInEx.*",
        "BepInEx",
        "MonoMod.*",
        "0Harmony",
        "HarmonyXInterop",
        "StructureMap",
        "Newtonsoft.Json"
    };

    public static string[] AssemblyIncludeRaw { get; } =
    {
        "Unity.InputSystem",
        "UnityEngine.InputModule"
    };
}