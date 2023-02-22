using System;

namespace UniTAS.Patcher.PatcherUtils;

/// <summary>
/// Attribute to mark a method as a patcher
/// Make sure to match signature of either `static void SomeMethod(AssemblyDefinition)` or `static void SomeMethod(ref AssemblyDefinition)`
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PatcherAttribute : Attribute
{
    /// <summary>
    /// Priority on patcher execution order
    /// Higher priority will be executed first
    /// </summary>
    public int Priority { get; }

    public PatcherAttribute(int priority = 0)
    {
        Priority = priority;
    }
}