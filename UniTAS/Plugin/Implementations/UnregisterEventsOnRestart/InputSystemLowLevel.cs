using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.UnregisterEventsOnRestart;

namespace UniTAS.Plugin.Implementations.UnregisterEventsOnRestart;

[Singleton]
public class InputSystemLowLevel : EventsClearer
{
    protected override IEnumerable<FieldInfo> FieldsToClear()
    {
        var type = AccessTools.TypeByName("UnityEngineInternal.Input.NativeInputSystem");
        if (type == null) yield break;

        yield return AccessTools.Field(type, "onUpdate");
        yield return AccessTools.Field(type, "onBeforeUpdate");
        yield return AccessTools.Field(type, "s_OnDeviceDiscoveredCallback");
    }
}