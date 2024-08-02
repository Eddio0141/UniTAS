using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class InputSystemState : IInputSystemState
{
    public bool HasNewInputSystem { get; }
    public bool HasOldInputSystem { get; }

    public InputSystemState(IPatchReverseInvoker patchReverseInvoker)
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
    }
}
