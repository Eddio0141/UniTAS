using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class HarmonyStorage : IHarmony
{
    public Harmony Harmony { get; } = new("dev.yuu0141.unitas");
}