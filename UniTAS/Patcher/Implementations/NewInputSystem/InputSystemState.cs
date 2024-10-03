using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class InputSystemState : IInputSystemState
{
    public bool HasNewInputSystem { get; }
    public bool HasOldInputSystem { get; }
    public bool HasRewired { get; }

    public InputSystemState(IPatchReverseInvoker patchReverseInvoker, ILogger logger)
    {
        try
        {
            var mouseCurrent = AccessTools.TypeByName("UnityEngine.InputSystem.Mouse");
            var current = AccessTools.Property(mouseCurrent, "current");
            current.GetValue(null, null);

            HasNewInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            patchReverseInvoker.Invoke(() => Input.GetKeyDown(KeyCode.A));
            HasOldInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }

        HasRewired = AccessTools.TypeByName("Rewired.ReInput") != null;
        
        logger.LogMessage($"Has unity new input system: {HasNewInputSystem}");
        logger.LogMessage($"Has unity old input system: {HasOldInputSystem}");
        logger.LogMessage($"Has rewired input system: {HasRewired}");
    }
}
