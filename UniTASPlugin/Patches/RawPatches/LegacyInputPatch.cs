using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Patches.PatchTypes;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global

// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.RawPatches;

[RawPatch]
public class LegacyInputPatch
{
    private static readonly IReverseInvokerFactory ReverseInvokerFactory =
        Plugin.Kernel.GetInstance<IReverseInvokerFactory>();

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

        private static bool Prefix(object key, ref bool __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.Keys.Contains((int)(KeyCode)key);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
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

        private static bool Prefix(object key, ref bool __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.KeysUp.Contains((int)(KeyCode)key);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
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

        private static bool Prefix(object key, ref bool __result)
        {
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            __result = VirtualEnvironment.InputState.KeyboardState.KeysDown.Contains((int)(KeyCode)key);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            if (VirtualEnvironment.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }

            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            if (VirtualEnvironment.InputState.AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }

            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var mouseState = VirtualEnvironment.InputState.MouseState;
            __result = new(mouseState.XPos, mouseState.YPos);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            var mouseState = VirtualEnvironment.InputState.MouseState;
            ret = new(mouseState.XPos, mouseState.YPos);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            // TODO make this work
            // Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
            // TODO also make sure movie overwrites input on the same frame after reset
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;
            VirtualEnvironment.InputState.AxisState.Values.Clear();
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.Buttons.Contains(buttonName);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.ButtonsDown.Contains(buttonName);
            return false;
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
            if (ReverseInvokerFactory.GetReverseInvoker().Invoking)
                return true;
            if (!VirtualEnvironment.RunVirtualEnvironment) return true;

            __result = VirtualEnvironment.InputState.ButtonState.ButtonsUp.Contains(buttonName);
            return false;
        }
    }
}