using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.InputSystemOverride;

namespace UniTAS.Patcher.Implementations.RewiredFix;

[Singleton]
[ForceInstantiate]
public class FallbackToUnityInput
{
    public FallbackToUnityInput(IInputSystemState inputSystemState)
    {
        if (!inputSystemState.HasRewired) return;

        var reInput = AccessTools.TypeByName("Rewired.ReInput");

        if (reInput == null)
        {
            throw new Exception("failed to find Rewired.ReInput type, cannot apply fix");
        }

        var configuration = AccessTools.Property(reInput, "configuration")?.GetValue(null, null);

        if (configuration == null)
        {
            throw new Exception("failed to find Rewired.ReInput.configuration property, cannot apply fix");
        }

        var alwaysUseUnityInput = AccessTools.Property(configuration.GetType(), "alwaysUseUnityInput");

        if (alwaysUseUnityInput == null)
        {
            throw new Exception(
                "failed to find Rewired.ReInput.configuration.alwaysUseUnityInput property, cannot apply fix");
        }

        alwaysUseUnityInput.SetValue(configuration, true, null);
    }
}