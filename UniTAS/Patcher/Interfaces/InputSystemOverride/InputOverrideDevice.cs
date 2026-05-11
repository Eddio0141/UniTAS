using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

[RegisterAll]
public abstract class InputOverrideDevice(IPatchReverseInvoker reverseInvoker)
{
    private static readonly MethodInfo AddDeviceGeneric = AccessTools.DeclaredMethod(typeof(InputSystem), nameof(InputSystem.AddDevice), [typeof(string)]);
    private static readonly object[] AddDeviceArgs = [null];
    private static Type InputRuntime = AccessTools.TypeByName("UnityEngine.InputSystem.LowLevel.InputRuntime");
    private static FieldInfo InputRuntimeInstance = AccessTools.Field(InputRuntime, "s_Instance");
    private static Type NativeInputRuntime = AccessTools.TypeByName("UnityEngine.InputSystem.LowLevel.NativeInputRuntime");
    private static MethodInfo InputRuntimeCurrentTime = AccessTools.PropertyGetter(NativeInputRuntime, "currentTime");

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

    /// <summary>
    /// A wrapper around <see cref="InputSystem.QueueStateEvent"/>
    /// </summary>
    protected void QueueStateEvent<TState>(TState state)
        where TState : struct, IInputStateTypeInfo
    {
        // var time = (double)reverseInvoker.Invoke(() => InputRuntimeCurrentTime.Invoke(InputRuntimeInstance.GetValue(null), null));
        // InputSystem.QueueStateEvent(Device, state, time);
        InputSystem.QueueStateEvent(Device, state);
    }
}
