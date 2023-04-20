using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Implementations.VirtualEnvironment;
using UniTAS.Plugin.Interfaces.Patches.PatchTypes;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable UnusedMember.Local

namespace UniTAS.Plugin.Patches;

[RawPatch]
public class LegacyInputPatch
{
    private static readonly IPatchReverseInvoker ReverseInvoker =
        Plugin.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly VirtualEnvController VirtualEnvController =
        Plugin.Kernel.GetInstance<VirtualEnvController>();

    private static readonly IKeyboardStateEnv KeyboardStateEnv =
        Plugin.Kernel.GetInstance<IKeyboardStateEnv>();

    private static readonly IButtonStateEnv ButtonStateEnv =
        Plugin.Kernel.GetInstance<IButtonStateEnv>();

    private static readonly IMouseStateEnv MouseStateEnv =
        Plugin.Kernel.GetInstance<IMouseStateEnv>();

    private static readonly IAxisStateEnv AxisStateEnv =
        Plugin.Kernel.GetInstance<IAxisStateEnv>();

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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = KeyboardStateEnv.Keys.Any(x => x.KeyCode.HasValue && x.KeyCode == key);
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

        private static bool Prefix(string name, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            if (!LegacyInputSystemUtils.KeyStringToKeyCode(name, out var foundKeyCode))
            {
                __result = KeyboardStateEnv.Keys.Any(x => !x.KeyCode.HasValue && x.Keys == name);
                return false;
            }

            __result = KeyboardStateEnv.Keys.Any(x => x.KeyCode.HasValue && x.KeyCode == foundKeyCode);
            return false;
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

        private static bool Prefix(string name, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            if (!LegacyInputSystemUtils.KeyStringToKeyCode(name, out var foundKeyCode))
            {
                __result = KeyboardStateEnv.KeysUp.Any(x => !x.KeyCode.HasValue && x.Keys == name);
                return false;
            }

            __result = KeyboardStateEnv.KeysUp.Any(x => x.KeyCode.HasValue && x.KeyCode == foundKeyCode);
            return false;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = KeyboardStateEnv.KeysUp.Any(x => x.KeyCode.HasValue && x.KeyCode == key);
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

        private static bool Prefix(string name, ref bool __result)
        {
            if (ReverseInvoker.InnerCall())
                return true;
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            if (!LegacyInputSystemUtils.KeyStringToKeyCode(name, out var foundKeyCode))
            {
                __result = KeyboardStateEnv.KeysDown.Any(x => !x.KeyCode.HasValue && x.Keys == name);
                return false;
            }

            __result = KeyboardStateEnv.KeysDown.Any(x => x.KeyCode.HasValue && x.KeyCode == foundKeyCode);
            return false;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = KeyboardStateEnv.KeysDown.Any(x => x.KeyCode.HasValue && x.KeyCode == key);
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            if (AxisStateEnv.Values.TryGetValue(axisName, out var value))
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            if (AxisStateEnv.Values.TryGetValue(axisName, out var value))
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            var mouseState = MouseStateEnv;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            var mouseState = MouseStateEnv;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => MouseStateEnv.LeftClick,
                1 => MouseStateEnv.RightClick,
                2 => MouseStateEnv.MiddleClick,
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => MouseStateEnv.LeftClickDown,
                1 => MouseStateEnv.RightClickDown,
                2 => MouseStateEnv.MiddleClickDown,
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = button switch
            {
                0 => MouseStateEnv.LeftClickUp,
                1 => MouseStateEnv.RightClickUp,
                2 => MouseStateEnv.MiddleClickUp,
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            AxisStateEnv.Values.Clear();
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;

            __result = ButtonStateEnv.Buttons.Contains(buttonName);
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;

            __result = ButtonStateEnv.ButtonsDown.Contains(buttonName);
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;

            __result = ButtonStateEnv.ButtonsUp.Contains(buttonName);
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = MouseStateEnv.MousePresent;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = KeyboardStateEnv.KeysDown.Count > 0 || KeyboardStateEnv.Keys.Count > 0 ||
                       MouseStateEnv.LeftClick ||
                       MouseStateEnv.RightClick || MouseStateEnv.MiddleClick;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            __result = KeyboardStateEnv.KeysDown.Count > 0 || MouseStateEnv.LeftClickDown ||
                       MouseStateEnv.RightClickDown || MouseStateEnv.MiddleClickDown;
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
            return !VirtualEnvController.RunVirtualEnvironment;
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO this also does the "repeat when held" thing
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
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
            return !VirtualEnvController.RunVirtualEnvironment;
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
            if (!VirtualEnvController.RunVirtualEnvironment) return true;
            // TODO what is this call
            __result = Quaternion.identity;
            return false;
        }
    }
}