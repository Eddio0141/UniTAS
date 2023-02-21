using System;

namespace UniTAS.Patcher.PatcherUtils;

/// <summary>
/// Attribute to mark a method as a patcher
/// Make sure to match signature of either `static void SomeMethod(AssemblyDefinition)` or `static void SomeMethod(ref AssemblyDefinition)`
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PatcherAttribute : Attribute
{
}