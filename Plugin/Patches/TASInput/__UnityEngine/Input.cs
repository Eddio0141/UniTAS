using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.TAS.Input;
using UnityEngine;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine;

[HarmonyPatch(typeof(Input))]
class InputPatch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Auxilary.Cleanup_IgnoreNotFound(original, ex);
    }

    // TODO not sure what this is
    /*
    [HarmonyPrefix]
    [HarmonyPatch("CheckDisabled")]
    static void Prefix_CheckDisabled(ref int __result)
    {
    }
    */

    [HarmonyPrefix]
    [HarmonyPatch("penEventCount", MethodType.Getter)]
    static bool Prefix_penEventCountGetter(ref int __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            __result = 0;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("mousePresent", MethodType.Getter)]
    static bool Prefix_mousePresentGetter(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            // TODO option to present mouse
            __result = true;
            return false;
        }
        return true;
    }

    // TODO does the ret value work with ref?
    [HarmonyPrefix]
    [HarmonyPatch("GetPenEvent_Injected", new Type[] { typeof(int) })]
    static bool Prefix_GetPenEvent_Injected(int index, ref object ret)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    // above calls GetPenEvent

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.mainGyroIndex_Internal))]
    static bool Prefix_mainGyroIndex_Internal(ref int __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetPosition))]
    static bool Prefix_GetPosition(int deviceID, ref Vector3 __result)
    {
        if (TAS.Main.Running)
        {
            // TODO what is this function call
            return false;
        }
        return true;
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyInt))]
    static bool Prefix_GetKeyInt(object key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.Keys.Contains((KeyCode)key);
            return false;
        }
        return true;
    }

    // above gets called from GetKey

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyString))]
    static bool Prefix_GetKeyString(/*string name, ref bool __result*/)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    // above gets called from GetKey

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyUpString))]
    static bool Prefix_GetKeyUpString(/*string name, ref bool __result*/)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    // above gets called from GetKeyUp

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyUpInt))]
    static bool Prefix_GetKeyUpInt(object key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.KeysUp.Contains((KeyCode)key);
            return false;
        }
        return true;
    }

    // above gets called from GetKeyUp

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyDownString))]
    static bool Prefix_GetKeyDownString(/*string name*/)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }

        return true;
    }

    // above gets called from GetKeyDown

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetKeyDownInt))]
    static bool Prefix_GetKeyDownInt(object key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.KeysDown.Contains((KeyCode)key);
            return false;
        }
        return true;
    }

    // above gets called from GetKeyDown

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetAxis))]
    static bool Prefix_GetAxis(string axisName, ref float __result)
    {
        if (TAS.Main.Running)
        {
            if (Axis.Values.TryGetValue(axisName, out float value))
            {
                __result = value;
            }
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetAxisRaw))]
    static bool Prefix_GetAxisRaw(string axisName, ref float __result)
    {
        if (TAS.Main.Running)
        {
            if (Axis.Values.TryGetValue(axisName, out float value))
            {
                // TODO whats diff between Raw and normal
                __result = value;
            }
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetButton))]
    static bool Prefix_GetButton(string buttonName)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetButtonDown))]
    static bool Prefix_GetButtonDown(string buttonName)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetButtonUp))]
    static bool Prefix_GetButtonUp(string buttonName)
    {
        if (TAS.Main.Running)
        {
            // TODO
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetMouseButton))]
    static bool Prefix_GetMouseButton(ref bool __result, int button)
    {
        if (TAS.Main.Running)
        {
            __result = button switch
            {
                0 => Mouse.LeftClick,
                1 => Mouse.RightClick,
                2 => Mouse.MiddleClick,
                _ => false,
            };
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetMouseButtonDown))]
    static bool Prefix_GetMouseButtonDown(ref bool __result, int button)
    {
        if (TAS.Main.Running)
        {
            __result = button switch
            {
                0 => Mouse.LeftClickDown,
                1 => Mouse.RightClickDown,
                2 => Mouse.MiddleClickDown,
                _ => false,
            };
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetMouseButtonUp))]
    static bool Prefix_GetMouseButtonUp(ref bool __result, int button)
    {
        if (TAS.Main.Running)
        {
            __result = button switch
            {
                0 => Mouse.LeftClickUp,
                1 => Mouse.RightClickUp,
                2 => Mouse.MiddleClickUp,
                _ => false,
            };
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.ResetInputAxes))]
    static bool Prefix_ResetInputAxes()
    {
        // TODO make this work
        // Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
        // TODO also make sure movie overwrites input on the same frame after reset
        if (TAS.Main.Running)
        {
            Axis.Values.Clear();
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetAccelerationEvent))]
    static bool Prefix_GetAccelerationEvent(int index, ref AccelerationEvent __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            // this gets called in accelerationEvents getter, check when implementing
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.anyKey), MethodType.Getter)]
    static bool Prefix_anyKeyGetter(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.Keys.Count > 0 || Mouse.LeftClick || Mouse.RightClick || Mouse.MiddleClick;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.anyKeyDown), MethodType.Getter)]
    static bool Prefix_anyKeyDownGetter(ref bool __result)
    {
        // TODO make sure this gets called before Update calls
        if (TAS.Main.Running)
        {
            __result = Keyboard.KeysDown.Count > 0 || Mouse.LeftClickDown || Mouse.RightClickDown || Mouse.MiddleClickDown;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.inputString), MethodType.Getter)]
    static bool Prefix_inputStringGetter()
    {
        if (TAS.Main.Running)
        {
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.mousePosition), MethodType.Getter)]
    static bool Prefix_mousePositionGetter(ref Vector3 __result)
    {
        if (TAS.Main.Running)
        {
            __result = Mouse.Position;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.multiTouchEnabled), MethodType.Getter)]
    static bool Prefix_multiTouchEnabledGetter(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.multiTouchEnabled), MethodType.Setter)]
    static bool Prefix_multiTouchEnabledSetter(bool value)
    {
        if (TAS.Main.Running)
        {
            // TODO handle this
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.isGyroAvailable), MethodType.Getter)]
    static bool Prefix_isGyroAvailableGetter(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.deviceOrientation), MethodType.Getter)]
    static bool Prefix_deviceOrientationGetter(ref DeviceOrientation __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.acceleration), MethodType.Getter)]
    static bool Prefix_accelerationGetter(ref Vector3 __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            __result = Vector3.zero;
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.accelerationEventCount), MethodType.Getter)]
    static bool Prefix_accelerationEventCountGetter(ref int __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            // this gets called in accelerationEvents getter, check there if implementing
            __result = 0;
            return false;
        }
        return true;
    }

    /*
    public static AccelerationEvent[] accelerationEvents
    {
        get
        {
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.touchCount), MethodType.Getter)]
    static bool Prefix_touchCountGetter(ref int __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetTouch))]
    static bool Prefix_GetTouch(ref Touch __result)
    {
        if (TAS.Main.Running)
        {
            // TODO
            // this gets called in touches getter, check if implementing
            return false;
        }
        return true;
    }

    /*
    public static Touch[] touches
    {
        get
        {
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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Input.GetRotation))]
    static bool Prefix_GetRotation(ref Vector3 __result)
    {
        if (TAS.Main.Running)
        {
            // TODO what is this call
            __result = Vector3.zero;
            return false;
        }
        return true;
    }
}