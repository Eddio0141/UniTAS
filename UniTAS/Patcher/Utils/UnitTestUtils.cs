using HarmonyLib;

namespace UniTAS.Patcher.Utils;

public static class UnitTestUtils
{
    public static bool IsTesting { get; } = AccessTools.TypeByName("Xunit.FactAttribute") != null;
}