using System;
using System.Reflection;
using HarmonyLib;
using Ninject;
using UniTASPlugin.GameEnvironment;
using UnityEngine;
using InputOrig = UnityEngine.Input;

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
    class penEventCountGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), "mousePresent", MethodType.Getter)]
    class mousePresentGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO option to present mouse
            __result = true;
            return false;
        }
    }

    // TODO does the ret value work with ref?
    [HarmonyPatch(typeof(InputOrig), "GetPenEvent_Injected", typeof(int))]
    class GetPenEvent_Injected
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(int index, ref object ret)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above calls GetPenEvent

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.mainGyroIndex_Internal))]
    class mainGyroIndex_Internal
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetPosition))]
    class GetPosition
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(int deviceID, ref Vector3 __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class GetKeyInt
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(object key, ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.Keys.Contains((KeyCode)key);
            return false;
        }
    }

    // above gets called from GetKey

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyString))]
    class GetKeyString
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(/*string name, ref bool __result*/)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKey

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyUpString))]
    class GetKeyUpString
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(/*string name, ref bool __result*/)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKeyUp

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyUpInt))]
    class GetKeyUpInt
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(object key, ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.KeysUp.Contains((KeyCode)key);
            return false;
        }
    }

    // above gets called from GetKeyUp

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyDownString))]
    class GetKeyDownString
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(/*string name*/)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    // above gets called from GetKeyDown

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetKeyDownInt))]
    class GetKeyDownInt
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(object key, ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.KeyboardState.KeysDown.Contains((KeyCode)key);
            return false;
        }
    }

    // above gets called from GetKeyDown

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAxis))]
    class GetAxis
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string axisName, ref float __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            if (env.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAxisRaw))]
    class GetAxisRaw
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string axisName, ref float __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class GetButton
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string buttonName)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetButtonDown))]
    class GetButtonDown
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string buttonName)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetButtonUp))]
    class GetButtonUp
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(string buttonName)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetMouseButton))]
    class GetMouseButton
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, int button)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class GetMouseButtonDown
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, int button)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class GetMouseButtonUp
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result, int button)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class ResetInputAxes
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            // TODO make this work
            // Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
            // TODO also make sure movie overwrites input on the same frame after reset
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            env.InputState.AxisState.Values.Clear();
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAccelerationEvent))]
    class GetAccelerationEvent
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(int index, ref AccelerationEvent __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO
            // this gets called in accelerationEvents getter, check when implementing
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKey), MethodType.Getter)]
    class anyKeyGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var inputState = env.InputState;
            __result = inputState.KeyboardState.Keys.Count > 0 || inputState.MouseState.LeftClick || inputState.MouseState.RightClick || inputState.MouseState.MiddleClick;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKeyDown), MethodType.Getter)]
    class anyKeyDownGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result)
        {
            // TODO make sure this gets called before Update calls
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            var inputState = env.InputState;
            __result = inputState.KeyboardState.KeysDown.Count > 0 || inputState.MouseState.LeftClickDown || inputState.MouseState.RightClickDown || inputState.MouseState.MiddleClickDown;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.inputString), MethodType.Getter)]
    class inputStringGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix()
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.mousePosition), MethodType.Getter)]
    class mousePositionGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref Vector3 __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            __result = env.InputState.MouseState.Position;
            return false;
        }
    }

#pragma warning disable Harmony002 // why does unity name things like this
    [HarmonyPatch(typeof(InputOrig), "get_mousePosition_Injected", MethodType.Normal)]
#pragma warning restore Harmony002
    class get_mousePosition_Injected
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref Vector3 ret)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            ret = env.InputState.MouseState.Position;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Getter)]
    class multiTouchEnabledGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;

        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Setter)]
    class multiTouchEnabledSetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(bool value)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            return !env.RunVirtualEnvironment;
            // TODO handle this
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.isGyroAvailable), MethodType.Getter)]
    class isGyroAvailableGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref bool __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;

        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.deviceOrientation), MethodType.Getter)]
    class deviceOrientationGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref DeviceOrientation __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.acceleration), MethodType.Getter)]
    class accelerationGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref Vector3 __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            __result = Vector3.zero;
            return false;

        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.accelerationEventCount), MethodType.Getter)]
    class accelerationEventCountGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class touchCountGetter
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref int __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetTouch))]
    class GetTouch
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref Touch __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
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
    class GetRotation
    {
        static Exception Cleanup(MethodBase original, Exception ex)
        {
            return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        static bool Prefix(ref Vector3 __result)
        {
            var env = Plugin.Instance.Kernel.Get<VirtualEnvironment>();
            if (!env.RunVirtualEnvironment) return true;
            // TODO what is this call
            __result = Vector3.zero;
            return false;
        }
    }
}
