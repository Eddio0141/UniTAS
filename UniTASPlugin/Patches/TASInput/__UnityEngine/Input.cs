using HarmonyLib;
using UniTASPlugin.TAS.Input;
using UnityEngine;

namespace UniTASPlugin.Patches.TASInput.__UnityEngine;

#pragma warning disable CS0618
#pragma warning disable IDE1006

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyInt))]
class GetKeyInt
{
    static KeyCode lastKey;

    static bool Prefix(KeyCode key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.Keys.Contains(key);

            return false;
        }

        return true;
    }

    static void Postfix(KeyCode key, ref bool __result)
    {
        if (__result && key != lastKey)
        {
            if (UnityEngine.Time.captureDeltaTime != 0)
                Plugin.Log.LogDebug($"GetKeyInt({key}) = true, frametime: {UnityEngine.Time.captureDeltaTime}");
            lastKey = key;
        }
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyString))]
class GetKeyString
{
    static bool Prefix(/*string name, ref bool __result*/)
    {
        if (TAS.Main.Running)
        {
            //Plugin.Log.LogDebug($"GetKeyString: {name}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpInt))]
class GetKeyUpInt
{
    static KeyCode lastKey;

    static bool Prefix(KeyCode key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.KeysUp.Contains(key);

            return false;
        }

        return true;
    }

    static void Postfix(KeyCode key, ref bool __result)
    {
        if (__result && key != lastKey)
        {
            if (UnityEngine.Time.captureDeltaTime != 0)
                Plugin.Log.LogDebug($"GetKeyUpInt({key}) = true, frametime: {UnityEngine.Time.captureDeltaTime}");
            lastKey = key;
        }
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyUpString))]
class GetKeyUpString
{
    static bool Prefix(/*string name, ref bool __result*/)
    {
        if (TAS.Main.Running)
        {
            //Plugin.Log.LogDebug($"GetKeyUpString: {name}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownInt))]
class GetKeyDownInt
{
    static KeyCode lastKey;

    static bool Prefix(KeyCode key, ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Keyboard.KeysDown.Contains(key);

            return false;
        }

        return true;
    }

    static void Postfix(KeyCode key, ref bool __result)
    {
        if (__result && key != lastKey)
        {
            if (UnityEngine.Time.captureDeltaTime != 0)
                Plugin.Log.LogDebug($"GetKeyDownInt({key}) = true, frametime: {UnityEngine.Time.captureDeltaTime}");
            lastKey = key;
        }
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetKeyDownString))]
class GetKeyDownString
{
    static bool Prefix(/*string name*/)
    {
        if (TAS.Main.Running)
        {
            //Plugin.Log.LogDebug($"GetKeyDownString: {name}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetAxis))]
class GetAxis
{
    static bool Prefix(string axisName, ref float __result)
    {
        TAS.Main.AxisCall(axisName);

        if (TAS.Main.Running)
        {
            // TODO some notification on missing axis
            if (Axis.Values.TryGetValue(axisName, out float value))
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
    static bool Prefix(string axisName, ref float __result)
    {
        TAS.Main.AxisCall(axisName);

        if (TAS.Main.Running)
        {
            // TODO some notification on missing axis
            // TODO find out what the difference between GetAxis and GetAxisRaw is
            if (Axis.Values.TryGetValue(axisName, out float value))
            {
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
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetButtonDown))]
class GetButtonDown
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetButtonUp))]
class GetButtonUp
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButton))]
class GetMouseButton
{
    static bool Prefix(ref bool __result, int button)
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
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonDown))]
class GetMouseButtonDown
{
    static bool Prefix(ref bool __result, int button)
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
}

[HarmonyPatch(typeof(Input), nameof(Input.GetMouseButtonUp))]
class GetMouseButtonUp
{
    static bool Prefix(ref bool __result, int button)
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
}

[HarmonyPatch(typeof(Input), nameof(Input.ResetInputAxes))]
class ResetInputAxes
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetJoystickNames))]
class GetJoystickNames
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetTouch))]
class GetTouch
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetAccelerationEvent))]
class GetAccelerationEvent
{
    static bool Prefix(int index)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.GetAccelerationEvent called with value: {index}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.SimulateTouch))]
class SimulateTouch
{
    static bool Prefix(Touch touch)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.SimulateTouch called with value: {touch}");

        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.SimulateTouchInternal))]
class SimulateTouchInternal
{
    static bool Prefix(Touch touch, long timestamp)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.SimulateTouchInternal called with value: {touch}, {timestamp}");

        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.simulateMouseWithTouches), MethodType.Getter)]
class simulateMouseWithTouchesGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.simulateMouseWithTouches), MethodType.Setter)]
class simulateMouseWithTouchesSetter
{
    static bool Prefix(bool value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.simulateMouseWithTouches Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.anyKey), MethodType.Getter)]
class anyKeyGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.anyKeyDown), MethodType.Getter)]
class anyKeyDownGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.inputString), MethodType.Getter)]
class inputStringGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.mousePosition), MethodType.Getter)]
class mousePositionGetter
{
    static bool Prefix(ref Vector3 __result)
    {
        if (TAS.Main.Running)
        {
            __result = Mouse.Position;
            return false;
        }
        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.mouseScrollDelta), MethodType.Getter)]
class mouseScrollDeltaGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.imeCompositionMode), MethodType.Getter)]
class imeCompositionModeGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.imeCompositionMode), MethodType.Setter)]
class imeCompositionModeSetter
{
    static bool Prefix(IMECompositionMode value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.imeCompositionMode Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compositionString), MethodType.Getter)]
class compositionStringGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.imeIsSelected), MethodType.Getter)]
class imeIsSelectedGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compositionCursorPos), MethodType.Getter)]
class compositionCursorPosGetter
{
    static bool Prefix(ref Vector2 __result)
    {
        if (TAS.Main.Running)
        {
            __result = Mouse.Position;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compositionCursorPos), MethodType.Setter)]
class compositionCursorPosSetter
{
    static bool Prefix(Vector2 value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.compositionCursorPos Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.eatKeyPressOnTextFieldFocus), MethodType.Getter)]
class eatKeyPressOnTextFieldFocusGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.eatKeyPressOnTextFieldFocus), MethodType.Setter)]
class eatKeyPressOnTextFieldFocusSetter
{
    static bool Prefix(bool value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.eatKeyPressOnTextFieldFocus Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.mousePresent), MethodType.Getter)]
class mousePresentGetter
{
    static bool Prefix(ref bool __result)
    {
        if (TAS.Main.Running)
        {
            __result = Mouse.MousePresent;

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.touchCount), MethodType.Getter)]
class touchCountGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.touchPressureSupported), MethodType.Getter)]
class touchPressureSupportedGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.stylusTouchSupported), MethodType.Getter)]
class stylusTouchSupportedGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.touchSupported), MethodType.Getter)]
class touchSupportedGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Getter)]
class multiTouchEnabledGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.multiTouchEnabled), MethodType.Setter)]
class multiTouchEnabledSetter
{
    static bool Prefix(bool value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.multiTouchEnabled Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.isGyroAvailable), MethodType.Getter)]
class isGyroAvailableGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.deviceOrientation), MethodType.Getter)]
class deviceOrientationGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.acceleration), MethodType.Getter)]
class accelerationGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compensateSensors), MethodType.Getter)]
class compensateSensorsGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compensateSensors), MethodType.Setter)]
class compensateSensorsSetter
{
    static bool Prefix(bool value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.compensateSensors Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.accelerationEventCount), MethodType.Getter)]
class accelerationEventCountGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.backButtonLeavesApp), MethodType.Getter)]
class backButtonLeavesAppGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.backButtonLeavesApp), MethodType.Setter)]
class backButtonLeavesAppSetter
{
    static bool Prefix(bool value)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.backButtonLeavesApp Setter called with value: {value}");

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.location), MethodType.Getter)]
class locationGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.compass), MethodType.Getter)]
class compassGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetGyroInternal))]
class GetGyroInternal
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug("UnityEngine.Input.GetGyroInternal default value not set");
        }

        return true;
    }

    static void Postfix(ref int __result)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.GetGyroInternal return value {__result}");
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.gyro), MethodType.Getter)]
class gyroGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.touches), MethodType.Getter)]
class touchesGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.accelerationEvents), MethodType.Getter)]
class accelerationEventsGetter
{
    static bool Prefix()
    {
        if (TAS.Main.Running)
        {
            return false;
        }

        return true;
    }
}

/* Internal class, unknown if needs to be patched
 * [HarmonyPatch(typeof(Input), "CheckDisabled")]
class CheckDisabled
{
    static bool Prefix()
    {
        if (TAS.TASTool.Running)
        {
            Plugin.Log.LogDebug("UnityEngine.Input.CheckDisabled default value not set");
        }

        return true;
    }

    static void Postfix(ref bool __result)
    {
        Plugin.Log.LogDebug($"UnityEngine.Input.CheckDisabled return value {__result}");
    }
}*/

[HarmonyPatch(typeof(Input), nameof(Input.GetTouch_Injected))]
class GetTouch_Injected
{
    static bool Prefix(int index/*, out Touch ret*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.GetTouch_Injected called with arg {index}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.GetAccelerationEvent_Injected))]
class GetAccelerationEvent_Injected
{
    static bool Prefix(int index/*, out AccelerationEvent ret*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.GetAccelerationEvent_Injected called with arg {index}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.SimulateTouchInternal_Injected))]
class SimulateTouchInternal_Injected
{
    static bool Prefix(ref Touch touch, long timestamp)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.SimulateTouchInternal_Injected called with arg {touch}, {timestamp}");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.get_mousePosition_Injected))]
class get_mousePosition_Injected
{
    static bool Prefix(/*out Vector3 ret*/)
    {
        if (TAS.Main.Running)
        {
            // TODO set ret
            Plugin.Log.LogDebug($"UnityEngine.Input.get_mousePosition_Injected called, TODO set ret");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.get_mouseScrollDelta_Injected))]
class get_mouseScrollDelta_Injected
{
    static bool Prefix(/*out Vector2 ret*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.get_mouseScrollDelta_Injected called");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.get_compositionCursorPos_Injected))]
class get_compositionCursorPos_Injected
{
    static bool Prefix(/*out Vector2 ret*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.get_compositionCursorPos_Injected called");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.set_compositionCursorPos_Injected))]
class set_compositionCursorPos_Injected
{
    static bool Prefix(/*ref Vector2 value*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.set_compositionCursorPos_Injected called");

            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(Input), nameof(Input.get_acceleration_Injected))]
class get_acceleration_Injected
{
    static bool Prefix(/*out Vector3 ret*/)
    {
        if (TAS.Main.Running)
        {
            Plugin.Log.LogDebug($"UnityEngine.Input.get_acceleration_Injected called");

            return false;
        }

        return true;
    }
}
