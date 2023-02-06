using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;
using InputOrig = UnityEngine.Input;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace UniTASPlugin.LegacyPatches.UnityEngine;

[HarmonyPatch]
public class InputPatch
{
    private static readonly VirtualEnvironment VirtualEnvironment = Plugin.Kernel.GetInstance<VirtualEnvironment>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        Plugin.Kernel.GetInstance<IPatchReverseInvoker>();

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
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;

            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = 0;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), "mousePresent", MethodType.Getter)]
    private class mousePresentGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO option to present mouse
            __result = true;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    // TODO does the ret value work with ref?
    [HarmonyPatch(typeof(InputOrig), "GetPenEvent_Injected", typeof(int))]
    private class GetPenEvent_Injected
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(int index, ref object ret)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    // above calls GetPenEvent

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.mainGyroIndex_Internal))]
    private class mainGyroIndex_Internal
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetPosition))]
    private class GetPosition
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(int deviceID, ref Vector3 __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO what is this function call
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetAccelerationEvent))]
    private class GetAccelerationEvent
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(int index, ref AccelerationEvent __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
            // this gets called in accelerationEvents getter, check when implementing
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKey), MethodType.Getter)]
    private class anyKeyGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            var inputState = VirtualEnvironment.InputState;
            __result = inputState.KeyboardState.Keys.Count > 0 || inputState.MouseState.LeftClick ||
                       inputState.MouseState.RightClick || inputState.MouseState.MiddleClick;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.anyKeyDown), MethodType.Getter)]
    private class anyKeyDownGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            // TODO make sure this gets called before Update calls
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            var inputState = VirtualEnvironment.InputState;
            __result = inputState.KeyboardState.KeysDown.Count > 0 || inputState.MouseState.LeftClickDown ||
                       inputState.MouseState.RightClickDown || inputState.MouseState.MiddleClickDown;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.inputString), MethodType.Getter)]
    private class inputStringGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix()
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Getter)]
    private class multiTouchEnabledGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.multiTouchEnabled), MethodType.Setter)]
    private class multiTouchEnabledSetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(bool value)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO handle this
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.isGyroAvailable), MethodType.Getter)]
    private class isGyroAvailableGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = false;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.deviceOrientation), MethodType.Getter)]
    private class deviceOrientationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref DeviceOrientation __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.acceleration), MethodType.Getter)]
    private class accelerationGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            __result = Vector3.zero;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.accelerationEventCount), MethodType.Getter)]
    private class accelerationEventCountGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in accelerationEvents getter, check there if implementing
            __result = 0;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    /*
    public static AccelerationEvent[] accelerationEvents
    {
        get
        {
            if (reverseInvokerFactory.GetReverseInvoker() .Invoking)
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
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(InputOrig), nameof(InputOrig.GetTouch))]
    private class GetTouch
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref Touch __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            return !VirtualEnvironment.RunVirtualEnvironment;
            // TODO
            // this gets called in touches getter, check if implementing
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    /*
    public static Touch[] touches
    {
        get
        {
            if (reverseInvokerFactory.GetReverseInvoker() .Invoking)
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
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref Vector3 __result)
        {
            if (PatchReverseInvoker.InnerCall())
                return true;
            if (VirtualEnvironment.RunVirtualEnvironment) return true;
            // TODO what is this call
            __result = Vector3.zero;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }
}