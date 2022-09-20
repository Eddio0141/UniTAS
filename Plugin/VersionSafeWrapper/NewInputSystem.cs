using HarmonyLib;
using System;
using System.Linq;

namespace UniTASPlugin.VersionSafeWrapper;

public static class NewInputSystem
{
    const string NAMESPACE = "UnityEngine.InputSystem";

    static Traverse inputSystemTraverse()
    {
        return Traverse.Create(inputSystemType());
    }

    static Type inputSystemType()
    {
        return AccessTools.TypeByName($"{NAMESPACE}.InputSystem");
    }

    static Type inputDeviceType()
    {
        return AccessTools.TypeByName($"{NAMESPACE}.InputDevice");
    }

    public static void ConnectAllDevices()
    {
        // TODO make movie choose what devices to connect at any point (and automatically choose what to connect)
        Traverse inputTraverse = inputSystemTraverse();
        if (!inputTraverse.TypeExists())
            return;

        // make sure to not add if they already exist
        object[] devices = inputTraverse.Property("devices").Method("ToArray").GetValue<object[]>();
        string[] toAdd = new string[] { /*"Gamepad",*/ "FastKeyboard", "FastMouse" };
        Traverse removeDevice = inputTraverse.Method("RemoveDevice", new Type[] { inputDeviceType() });

        Plugin.Log.LogDebug(string.Join(", ", inputTraverse.Property("devices").Method("ToArray").GetValue<object[]>().Select(o => o.GetType().ToString()).ToArray()));

        foreach (object device in devices)
        {
            // TODO use method or something that wont call event
            removeDevice.GetValue(new object[] { device });
        }
        // TODO use method or something that wont call event
        //inputTraverse.Method("FlushDisconnectedDevices").GetValue();

        System.Reflection.MethodInfo addDevice = AccessTools.Method(inputSystemType(), "AddDevice", new Type[] { typeof(string) });

        foreach (string toAddName in toAdd)
        {
            string toAddNameFull = $"{NAMESPACE}.{toAddName}";
            Type toAddType = AccessTools.TypeByName(toAddNameFull);
            if (toAddType == null)
            {
                Plugin.Log.LogError($"{toAddNameFull} type is required but missing");
                continue;
            }

            System.Reflection.MethodInfo generic = addDevice.MakeGenericMethod(toAddType);
            Plugin.Log.LogDebug($"Adding {toAddType}");
            generic.Invoke(null, new object[] { null });
        }
        Type newbloodLegacyInput = AccessTools.TypeByName("NewBlood.LegacyInput");
        System.Reflection.MethodInfo generic2 = addDevice.MakeGenericMethod(newbloodLegacyInput);
        generic2.Invoke(null, new object[] { null });

        Plugin.Log.LogDebug(string.Join(", ", inputTraverse.Property("devices").Method("ToArray").GetValue<object[]>().Select(o => o.GetType().ToString()).ToArray()));
    }
}
