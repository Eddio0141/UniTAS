using System;
using System.Linq;
using HarmonyLib;

namespace UniTASPlugin.LegacySafeWrappers;

public static class NewInputSystem
{
    private const string NAMESPACE = "UnityEngine.InputSystem";

    private static readonly Type InputSystemType = AccessTools.TypeByName($"{NAMESPACE}.InputSystem");

    private static readonly Traverse InputSystemTraverse = Traverse.Create(InputSystemType);

    private static readonly bool InputSystemExists = InputSystemTraverse.TypeExists();

    private static readonly Type inputDeviceType = AccessTools.TypeByName($"{NAMESPACE}.InputDevice");

    public static void ConnectAllDevices()
    {
        // TODO make movie choose what devices to connect at any point (and automatically choose what to connect)
        if (!InputSystemExists)
            return;

        // make sure to not add if they already exist
        var devices = InputSystemTraverse.Property("devices").Method("ToArray").GetValue<object[]>();
        var toAdd = new[]
        {
            /*"Gamepad",*/ "FastKeyboard", "FastMouse"
        };
        var removeDevice = InputSystemTraverse.Method("RemoveDevice", new[] { inputDeviceType });

        Plugin.Log.LogDebug(string.Join(", ",
            InputSystemTraverse.Property("devices").Method("ToArray").GetValue<object[]>()
                .Select(o => o.GetType().ToString()).ToArray()));

        foreach (var device in devices)
        {
            // TODO use method or something that wont call event
            _ = removeDevice.GetValue(device);
        }
        // TODO use method or something that wont call event
        //inputTraverse.Method("FlushDisconnectedDevices").GetValue();

        var addDevice = AccessTools.Method(InputSystemType, "AddDevice", new[] { typeof(string) });

        foreach (var toAddName in toAdd)
        {
            var toAddNameFull = $"{NAMESPACE}.{toAddName}";
            var toAddType = AccessTools.TypeByName(toAddNameFull);
            if (toAddType == null)
            {
                Plugin.Log.LogError($"{toAddNameFull} type is required but missing");
                continue;
            }

            var generic = addDevice.MakeGenericMethod(toAddType);
            Plugin.Log.LogDebug($"Adding {toAddType}");
            _ = generic.Invoke(null, new object[] { null });
        }

        var newbloodLegacyInput = AccessTools.TypeByName("NewBlood.LegacyInput");
        var generic2 = addDevice.MakeGenericMethod(newbloodLegacyInput);
        _ = generic2.Invoke(null, new object[] { null });

        Plugin.Log.LogDebug(string.Join(", ",
            InputSystemTraverse.Property("devices").Method("ToArray").GetValue<object[]>()
                .Select(o => o.GetType().ToString()).ToArray()));
    }
}