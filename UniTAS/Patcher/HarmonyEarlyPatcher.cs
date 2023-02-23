using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace UniTAS.Patcher;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class HarmonyEarlyPatcher
{
    /// <summary>
    /// A method for patching harmony before the game starts
    /// </summary>
    public static void PatchHarmony()
    {
        Trace.Write("Patching harmony early!");
        
        var harmony = new HarmonyLib.Harmony("UniTAS.Patcher");
        harmony.PatchAll();
    }
}