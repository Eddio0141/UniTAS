using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations;

[Singleton(timing: RegisterTiming.Entry)]
public class HarmonyStorage : IHarmony
{
    public Harmony Harmony { get; } = new("dev.yuu0141.unitas");
}