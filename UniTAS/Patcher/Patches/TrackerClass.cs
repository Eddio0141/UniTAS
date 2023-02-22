using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.PatcherUtils;

namespace UniTAS.Patcher.Patches;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class TrackerClass
{
    private static bool _isPatched;

    // before patching DontDestroyOnLoad
    [Patcher(1000)]
    public static void Patch(AssemblyDefinition assembly)
    {
        if (_isPatched) return;
        _isPatched = true;

        assembly.AddType(typeof(Tracker));
    }
}