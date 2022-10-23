using HarmonyLib;
using Ninject;
using System;
using System.Reflection;
using UniTASPlugin.GameEnvironment.InnerState.Input;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

// TODO not sure what this is
/*
[HarmonyPrefix]
[HarmonyPatch("CheckDisabled")]
static void Prefix_CheckDisabled(ref int __result)
{
}
*/

[HarmonyPatch(typeof(Input), "penEventCount", MethodType.Getter)]
class penEventCountGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            __result = 0;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), "mousePresent", MethodType.Getter)]
class mousePresentGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO option to present mouse
            __result = true;
            return false;
        }
        return true;
    }
}

// TODO does the ret value work with ref?
[HarmonyPatch(typeof(Input), "GetPenEvent_Injected", new Type[] { typeof(int) })]
class GetPenEvent_Injected
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int index, ref object ret)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

// above calls GetPenEvent

[HarmonyPatch(typeof(Input), nameof(Input.mainGyroIndex_Internal))]
class mainGyroIndex_Internal
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetPosition))]
class GetPosition
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int deviceID, ref Vector3 __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO what is this function call
            return false;
        }
        return true;
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

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyInt))]
class GetKeyInt
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(object key, ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = KeyboardState.Keys.Contains((KeyCode)key);
            return false;
        }
        return true;
    }
}

// above gets called from GetKey

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyString))]
class GetKeyString
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(/*string name, ref bool __result*/)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

// above gets called from GetKey

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpString))]
class GetKeyUpString
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(/*string name, ref bool __result*/)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

// above gets called from GetKeyUp

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpInt))]
class GetKeyUpInt
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(object key, ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = KeyboardState.KeysUp.Contains((KeyCode)key);
            return false;
        }
        return true;
    }
}

// above gets called from GetKeyUp

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownString))]
class GetKeyDownString
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(/*string name*/)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }

        return true;
    }
}

// above gets called from GetKeyDown

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownInt))]
class GetKeyDownInt
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(object key, ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = KeyboardState.KeysDown.Contains((KeyCode)key);
            return false;
        }
        return true;
    }
}

// above gets called from GetKeyDown

[HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
class GetAxis
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string axisName, ref float __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            if (AxisState.Values.TryGetValue(axisName, out var value))
            {
                __result = value;
            }
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetAxisRaw))]
class GetAxisRaw
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string axisName, ref float __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            if (AxisState.Values.TryGetValue(axisName, out var value))
            {
                // TODO whats diff between Raw and normal
                __result = value;
            }
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetButton))]
class GetButton
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string buttonName)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
class GetButtonDown
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string buttonName)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp))]
class GetButtonUp
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string buttonName)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton))]
class GetMouseButton
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, int button)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = button switch
            {
                0 => MouseState.LeftClick,
                1 => MouseState.RightClick,
                2 => MouseState.MiddleClick,
                _ => false,
            };
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
class GetMouseButtonDown
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, int button)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = button switch
            {
                0 => MouseState.LeftClickDown,
                1 => MouseState.RightClickDown,
                2 => MouseState.MiddleClickDown,
                _ => false,
            };
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
class GetMouseButtonUp
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, int button)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = button switch
            {
                0 => MouseState.LeftClickUp,
                1 => MouseState.RightClickUp,
                2 => MouseState.MiddleClickUp,
                _ => false,
            };
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.ResetInputAxes))]
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
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            AxisState.Values.Clear();
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetAccelerationEvent))]
class GetAccelerationEvent
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(int index, ref AccelerationEvent __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            // this gets called in accelerationEvents getter, check when implementing
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.anyKey), MethodType.Getter)]
class anyKeyGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = KeyboardState.Keys.Count > 0 || MouseState.LeftClick || MouseState.RightClick || MouseState.MiddleClick;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.anyKeyDown), MethodType.Getter)]
class anyKeyDownGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        // TODO make sure this gets called before Update calls
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = KeyboardState.KeysDown.Count > 0 || MouseState.LeftClickDown || MouseState.RightClickDown || MouseState.MiddleClickDown;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.inputString), MethodType.Getter)]
class inputStringGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix()
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // Returns the keyboard input entered this frame
            // Only ASCII characters are contained in the inputString.
            // Character "\n" represents return or enter.
            // TODO
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.mousePosition), MethodType.Getter)]
class mousePositionGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref Vector3 __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            __result = MouseState.Position;
            return false;
        }
        return true;
    }
}

#pragma warning disable Harmony002 // why does unity name things like this
[HarmonyPatch(typeof(Input), "get_mousePosition_Injected", MethodType.Normal)]
#pragma warning restore Harmony002
class get_mousePosition_Injected
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref Vector3 ret)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            ret = MouseState.Position;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Getter)]
class multiTouchEnabledGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            __result = false;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Setter)]
class multiTouchEnabledSetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(bool value)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO handle this
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.isGyroAvailable), MethodType.Getter)]
class isGyroAvailableGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            __result = false;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.deviceOrientation), MethodType.Getter)]
class deviceOrientationGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref DeviceOrientation __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            __result = DeviceOrientation.Unknown;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.acceleration), MethodType.Getter)]
class accelerationGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref Vector3 __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            __result = Vector3.zero;
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.accelerationEventCount), MethodType.Getter)]
class accelerationEventCountGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            // this gets called in accelerationEvents getter, check there if implementing
            __result = 0;
            return false;
        }
        return true;
    }
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

[HarmonyPatch(typeof(Input), nameof(Input.touchCount), MethodType.Getter)]
class touchCountGetter
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref int __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            // this gets called in touches getter, check if implementing
            __result = 0;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetTouch))]
class GetTouch
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref Touch __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO
            // this gets called in touches getter, check if implementing
            return false;
        }
        return true;
    }
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

[HarmonyPatch(typeof(Input), nameof(Input.GetRotation))]
class GetRotation
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref Vector3 __result)
    {
        if (Plugin.Instance.Kernel.Get<MovieRunner<MovieScriptEngine>>().IsRunning)
        {
            // TODO what is this call
            __result = Vector3.zero;
            return false;
        }
        return true;
    }
}