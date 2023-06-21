using HarmonyLib;

namespace UniTAS.Patcher.Services;

public interface IHarmony
{
    Harmony Harmony { get; }
}