using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

[RegisterAll]
public abstract class InputOverrideDevice
{
    private static readonly MethodInfo AddDeviceGeneric = AccessTools.DeclaredMethod(typeof(InputSystem), nameof(InputSystem.AddDevice), [typeof(string)]);
    private static readonly object[] AddDeviceArgs = [null];

    public void AddDevice()
    {
        if (Device == null)
        {
            var addDevice = AddDeviceGeneric.MakeGenericMethod(InputControlLayout);
            Device = (InputDevice)addDevice.Invoke(null, AddDeviceArgs);
            return;
        }

        InputSystem.AddDevice(Device);
    }

    public void RemoveDevice()
    {
        InputSystem.RemoveDevice(Device);
    }

    public void MakeCurrent()
    {
        Device.MakeCurrent();
    }

    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    public abstract void Update();

    protected InputDevice Device { get; private set; }

    protected abstract Type InputControlLayout { get; }
}
