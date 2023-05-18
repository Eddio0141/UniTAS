using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.UnregisterEventsOnRestart;

namespace UniTAS.Plugin.Implementations.UnregisterEventsOnRestart;

[Singleton]
public class NavMeshClearEventOnRestart : EventsClearer
{
    protected override IEnumerable<FieldInfo> FieldsToClear()
    {
        var navMesh = AccessTools.TypeByName("UnityEngine.AI.NavMesh");
        if (navMesh == null) yield break;

        yield return AccessTools.Field(navMesh, "onPreUpdate");
    }
}