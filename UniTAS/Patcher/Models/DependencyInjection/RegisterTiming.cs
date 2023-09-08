namespace UniTAS.Patcher.Models.DependencyInjection;

public enum RegisterTiming
{
    /// <summary>
    /// Register when the tool is started. This is before any assemblies are loaded into memory other than mscorlib and essential assemblies
    /// </summary>
    Entry,

    /// <summary>
    /// Register when unity is initialized. This is the default. It is the safest timing as everything should be initialized by then
    /// </summary>
    UnityInit
}