using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class InputSystemState : IInputSystemState, IOnGameRestartResume
{
    public bool HasNewInputSystem { get; }
    public bool HasOldInputSystem { get; }
    public bool HasRewired { get; }

    private int _newInputSystemEventId;
    public int NewInputSystemEventId
    {
        get
        {
            var result = _newInputSystemEventId;
            _newInputSystemEventId++;
            return result;
        }
    }

    public InputSystemState(IPatchReverseInvoker patchReverseInvoker, ILogger logger)
    {
        try
        {
            var mouseCurrent = AccessTools.TypeByName("UnityEngine.InputSystem.Mouse");
            var current = AccessTools.Property(mouseCurrent, "current");
            current.GetValue(null, null);

            HasNewInputSystem = true;
        }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
        catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
        { }

        try
        {
            patchReverseInvoker.Invoke(() => Input.GetKeyDown(KeyCode.A));
            HasOldInputSystem = true;
        }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception
        catch (Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception
        { }

        HasRewired = AccessTools.TypeByName("Rewired.ReInput") != null;

        logger.LogMessage($"Has unity new input system: {HasNewInputSystem}");
        logger.LogMessage($"Has unity old input system: {HasOldInputSystem}");
        logger.LogMessage($"Has rewired input system: {HasRewired}");
    }

    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        _newInputSystemEventId = 0;
    }
}
