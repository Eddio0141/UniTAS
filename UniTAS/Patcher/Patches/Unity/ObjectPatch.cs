using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using UniTAS.Patcher.PatcherUtils;

namespace UniTAS.Patcher.Patches.Unity;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ObjectPatch
{
    [Patcher]
    public static void DontDestroyOnLoadPatch(AssemblyDefinition assembly)
    {
    }
}