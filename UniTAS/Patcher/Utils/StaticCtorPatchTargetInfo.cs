namespace UniTAS.Patcher.Utils;

public static class StaticCtorPatchTargetInfo
{
    public static string[] AssemblyExclusionsRaw { get; } =
    {
        // c# related
        "System.*",
        "System",
        "netstandard",
        "mscorlib",
        "Mono.*",
        "Mono"
    };
}