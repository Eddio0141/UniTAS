using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Patches.PatchTypes;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable UnusedMember.Local

namespace UniTAS.Plugin.Patches.RawPatches;

[RawPatch]
public class LegacyInputPatch
{
    private static readonly IPatchReverseInvoker ReverseInvoker =
        Plugin.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly VirtualEnvironment VirtualEnvironment =
        Plugin.Kernel.GetInstance<VirtualEnvironment>();

    // gets called from GetKey
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyInt))]
    private class GetKeyInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.Keys.Contains(key);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // gets called from GetKey
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyString))]
    private class GetKeyString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix( /*string name, ref bool __result*/)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // gets called from GetKeyUp
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpString))]
    private class GetKeyUpString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix( /*string name, ref bool __result*/)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // gets called from GetKeyUp
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpInt))]
    private class GetKeyUpInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.KeysUp.Contains(key);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // gets called from GetKeyDown
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownString))]
    private class GetKeyDownString
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix( /*string name*/)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // gets called from GetKeyDown
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownInt))]
    private class GetKeyDownInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(KeyCode key, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.KeysDown.Contains(key);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // above gets called from gyro { get; }
    /*
    public static Gyroscope gyro
    {
        get
        {
            if (Input.m_MainGyro == null)
            {
                Input.m_MainGyro = new Gyroscope(Input.mainGyroIndex_Internal());
            }
            return Input.m_MainGyro;
        }
    }
    */


    [HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
    private class GetAxis
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string axisName, ref float __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            if (VirtualEnvironment.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }

            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
    private class GetAxisRaw
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string axisName, ref float __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            if (VirtualEnvironment.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }

            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.mousePosition), MethodType.Getter)]
    private class mousePositionGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var mouseState = VirtualEnvironment.InputState.MouseState;
            __result = new(mouseState.XPos, mouseState.YPos);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

#pragma warning disable Harmony002 // why does unity name things like this
    [HarmonyPatch(typeof(Input), "get_mousePosition_Injected", MethodType.Normal)]
#pragma warning restore Harmony002
    private class get_mousePosition_Injected
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref Vector3 ret)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var mouseState = VirtualEnvironment.InputState.MouseState;
            ret = new(mouseState.XPos, mouseState.YPos);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton))]
    private class GetMouseButton
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => VirtualEnvironment.InputState.MouseState.LeftClick,
                1 => VirtualEnvironment.InputState.MouseState.RightClick,
                2 => VirtualEnvironment.InputState.MouseState.MiddleClick,
                _ => false
            };
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
    private class GetMouseButtonDown
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => VirtualEnvironment.InputState.MouseState.LeftClickDown,
                1 => VirtualEnvironment.InputState.MouseState.RightClickDown,
                2 => VirtualEnvironment.InputState.MouseState.MiddleClickDown,
                _ => false
            };
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
    private class GetMouseButtonUp
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result, int button)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => VirtualEnvironment.InputState.MouseState.LeftClickUp,
                1 => VirtualEnvironment.InputState.MouseState.RightClickUp,
                2 => VirtualEnvironment.InputState.MouseState.MiddleClickUp,
                _ => false
            };
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.ResetInputAxes))]
    private class ResetInputAxes
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            if (ReverseInvoker.InnerCall())
                return true;
            // TODO make this work
            // Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
            // TODO also make sure movie overwrites input on the same frame after reset
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            VirtualEnvironment.InputState.AxisState.Values.Clear();
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetButton))]
    private class GetButton
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string buttonName, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.Buttons.Contains(buttonName);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
    private class GetButtonDown
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string buttonName, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.ButtonsDown.Contains(buttonName);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp))]
    private class GetButtonUp
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(string buttonName, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.ButtonsUp.Contains(buttonName);
            return false;
        }

        private static void Postfix()
        {
            ReverseInvoker.Return();
        }
    }

    // TODO not sure what this is
    /*
    [HarmonyPrefix]
    [HarmonyPatch("CheckDisabled")]
    static void Prefix_CheckDisabled(ref int __result)
    {
    }
    */

    [HarmonyPatch(typeof(Input), "penEventCount", MethodType.Getter)]
    private class penEventCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO not 100% sure what this is
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), "mousePresent", MethodType.Getter)]
    private class mousePresentGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.MouseState.MousePresent;
            return false;
        }
    }

    // TODO
    [HarmonyPatch(typeof(Input), "GetPenEvent_Injected", typeof(int))]
    private class GetPenEvent_Injected
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
        }
    }

    // above calls GetPenEvent

    // TODO
    [HarmonyPatch(typeof(Input), nameof(Input.mainGyroIndex_Internal))]
    private class mainGyroIndex_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
        }
    }

    // TODO what is this function call
    [HarmonyPatch(typeof(Input), nameof(Input.GetPosition))]
    private class GetPosition
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
        }
    }

    // TODO
    // this gets called in accelerationEvents getter, check when implementing
    [HarmonyPatch(typeof(Input), nameof(Input.GetAccelerationEvent))]
    private class GetAccelerationEvent
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.anyKey), MethodType.Getter)]
    private class anyKeyGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var inputState = VirtualEnvironment.InputState;
            __result = inputState.KeyboardState.Keys.Count > 0 || inputState.MouseState.LeftClick ||
                       inputState.MouseState.RightClick || inputState.MouseState.MiddleClick;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.anyKeyDown), MethodType.Getter)]
    private class anyKeyDownGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var inputState = VirtualEnvironment.InputState;
            __result = inputState.KeyboardState.KeysDown.Count > 0 || inputState.MouseState.LeftClickDown ||
                       inputState.MouseState.RightClickDown || inputState.MouseState.MiddleClickDown;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.inputString), MethodType.Getter)]
    private class inputStringGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Getter)]
    private class multiTouchEnabledGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Setter)]
    private class multiTouchEnabledSetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO handle this
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.isGyroAvailable), MethodType.Getter)]
    private class isGyroAvailableGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.deviceOrientation), MethodType.Getter)]
    private class deviceOrientationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref DeviceOrientation __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.acceleration), MethodType.Getter)]
    private class accelerationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = Vector3.zero;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.accelerationEventCount), MethodType.Getter)]
    private class accelerationEventCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
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
            if (reverseInvokerFactory.GetReverseInvoker() .Invoking)
                return true;
            int accelerationEventCount = Input.accelerationEventCount;
            AccelerationEvent[] array = new AccelerationEvent[accelerationEventCount];
            for (int i = 0; i < accelerationEventCount; i++)
            {
                array[i] = Input.GetAccelerationEvent(i);
            }
            return array;
        }
    }
    */

    [HarmonyPatch(typeof(Input), nameof(Input.touchCount), MethodType.Getter)]
    private class touchCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(Input), nameof(Input.GetTouch))]
    private class GetTouch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix()
        {
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
            // this gets called in touches getter, check if implementing
        }
    }

    /*
    public static Touch[] touches
    {
        get
        {
            if (reverseInvokerFactory.GetReverseInvoker() .Invoking)
                return true;
            int touchCount = Input.touchCount;
            Touch[] array = new Touch[touchCount];
            for (int i = 0; i < touchCount; i++)
            {
                array[i] = Input.GetTouch(i);
            }
            return array;
        }
    }
    */

    [HarmonyPatch(typeof(Input), nameof(Input.GetRotation))]
    private class GetRotation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref Quaternion __result)
        {
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO what is this call
            __result = Quaternion.identity;
            return false;
        }
    }
}