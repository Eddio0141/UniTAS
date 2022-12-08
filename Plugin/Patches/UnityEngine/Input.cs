using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UnityEngine;
using InputOrig = UnityEngine.Input;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class Input
{
    // TODO not sure what this is
    /*
    [HarmonyPrefix]
    [HarmonyPatch("CheckDisabled")]
    static void Prefix_CheckDisabled(ref int __result)
    {
    }
    */

    [HarmonyPatch(typeof(InputOrig), "penEventCount", MethodType.Getter)]
    private class penEventCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return false;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), "mousePresent", MethodType.Getter)]
    private class mousePresentGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO option to present mouse
            __result = true;
            return false;
        }
    }

    // TODO does the ret value work with ref?
    [HarmonyPatch(typeof(InputOrig), "GetPenEvent_Injected", typeof(int))]
    private class GetPenEvent_Injected
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(int index, ref object ret)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above calls GetPenEvent

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.mainGyroIndex_Internal))]
    private class mainGyroIndex_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetPosition))]
    private class GetPosition
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(int deviceID, ref Vector3 __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO what is this function call
        }
    }

    // above gets called from gyro { get; }
    /*
    public static Gyroscope gyro
    {
        get
        {
            if (InputOrig.m_MainGyro == null)
            {
                InputOrig.m_MainGyro = new Gyroscope(InputOrig.mainGyroIndex_Internal());
            }
            return InputOrig.m_MainGyro;
        }
    }
    */

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyInt))]
    private class GetKeyInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(object key, ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.Keys.Contains((int)((KeyCode)key));
            return false;
        }
    }

    // above gets called from GetKey

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyString))]
    private class GetKeyString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix( /*string name, ref bool __result*/)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKey

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyUpString))]
    private class GetKeyUpString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix( /*string name, ref bool __result*/)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKeyUp

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyUpInt))]
    private class GetKeyUpInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(object key, ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.KeysUp.Contains((int)((KeyCode)key));
            return false;
        }
    }

    // above gets called from GetKeyUp

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyDownString))]
    private class GetKeyDownString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix( /*string name*/)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKeyDown

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyDownInt))]
    private class GetKeyDownInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(object key, ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.KeysDown.Contains((int)((KeyCode)key));
            return false;
        }
    }

    // above gets called from GetKeyDown

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAxis))]
    private class GetAxis
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string axisName, ref float __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            if (env.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAxisRaw))]
    private class GetAxisRaw
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string axisName, ref float __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            if (env.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                // TODO whats diff between Raw and normal
                __result = value;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetButton))]
    private class GetButton
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string buttonName)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetButtonDown))]
    private class GetButtonDown
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string buttonName)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetButtonUp))]
    private class GetButtonUp
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(string buttonName)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetMouseButton))]
    private class GetMouseButton
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => env.InputState.MouseState.LeftClick,
                1 => env.InputState.MouseState.RightClick,
                2 => env.InputState.MouseState.MiddleClick,
                _ => false,
            };
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetMouseButtonDown))]
    private class GetMouseButtonDown
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => env.InputState.MouseState.LeftClickDown,
                1 => env.InputState.MouseState.RightClickDown,
                2 => env.InputState.MouseState.MiddleClickDown,
                _ => false,
            };
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetMouseButtonUp))]
    private class GetMouseButtonUp
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => env.InputState.MouseState.LeftClickUp,
                1 => env.InputState.MouseState.RightClickUp,
                2 => env.InputState.MouseState.MiddleClickUp,
                _ => false,
            };
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.ResetInputAxes))]
    private class ResetInputAxes
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            // TODO make this work
            // Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
            // TODO also make sure movie overwrites input on the same frame after reset
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            env.InputState.AxisState.Values.Clear();
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAccelerationEvent))]
    private class GetAccelerationEvent
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(int index, ref AccelerationEvent __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
            // this gets called in accelerationEvents getter, check when implementing
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKey), MethodType.Getter)]
    private class anyKeyGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var inputState = env.InputState;
            __result = inputState.KeyboardState.Keys.Count > 0 || inputState.MouseState.LeftClick ||
                       inputState.MouseState.RightClick || inputState.MouseState.MiddleClick;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKeyDown), MethodType.Getter)]
    private class anyKeyDownGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            // TODO make sure this gets called before Update calls
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var inputState = env.InputState;
            __result = inputState.KeyboardState.KeysDown.Count > 0 || inputState.MouseState.LeftClickDown ||
                       inputState.MouseState.RightClickDown || inputState.MouseState.MiddleClickDown;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.inputString), MethodType.Getter)]
    private class inputStringGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.mousePosition), MethodType.Getter)]
    private class mousePositionGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var mouseState = env.InputState.MouseState;
            __result = new(mouseState.XPos, mouseState.YPos);
            return false;
        }
    }

#pragma warning disable Harmony002 // why does unity name things like this
    [HarmonyPatch(typeof(InputOrig), "get_mousePosition_Injected", MethodType.Normal)]
#pragma warning restore Harmony002
    private class get_mousePosition_Injected
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 ret)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var mouseState = env.InputState.MouseState;
            ret = new(mouseState.XPos, mouseState.YPos);
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Getter)]
    private class multiTouchEnabledGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Setter)]
    private class multiTouchEnabledSetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(bool value)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO handle this
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.isGyroAvailable), MethodType.Getter)]
    private class isGyroAvailableGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.deviceOrientation), MethodType.Getter)]
    private class deviceOrientationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref DeviceOrientation __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.acceleration), MethodType.Getter)]
    private class accelerationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = Vector3.zero;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.accelerationEventCount), MethodType.Getter)]
    private class accelerationEventCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in accelerationEvents getter, check there if implementing
            __result = 0;
            return false;
        }
    }

    /*
    public static AccelerationEvent[] accelerationEvents
    {
        get
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            int accelerationEventCount = InputOrig.accelerationEventCount;
            AccelerationEvent[] array = new AccelerationEvent[accelerationEventCount];
            for (int i = 0; i < accelerationEventCount; i++)
            {
                array[i] = InputOrig.GetAccelerationEvent(i);
            }
            return array;
        }
    }
    */

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.touchCount), MethodType.Getter)]
    private class touchCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetTouch))]
    private class GetTouch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref Touch __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
            // this gets called in touches getter, check if implementing
        }
    }

    /*
    public static Touch[] touches
    {
        get
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            int touchCount = InputOrig.touchCount;
            Touch[] array = new Touch[touchCount];
            for (int i = 0; i < touchCount; i++)
            {
                array[i] = InputOrig.GetTouch(i);
            }
            return array;
        }
    }
    */

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetRotation))]
    private class GetRotation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (Plugin.Kernel.Resolve<PatchReverseInvoker>().Invoking)
                return true;
            var env = Plugin.Kernel.Resolve<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO what is this call
            __result = Vector3.zero;
            return false;
        }
    }
}