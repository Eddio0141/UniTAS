using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.UnregisterEventsOnRestart;

namespace UniTAS.Plugin.Implementations.UnregisterEventsOnRestart;

[Singleton]
public class SceneManagerClearEventsOnRestart : EventsClearer
{
    protected override IEnumerable<FieldInfo> FieldsToClear()
    {
        var sceneManagerType = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");
        if (sceneManagerType == null) yield break;

        yield return AccessTools.Field(sceneManagerType, "activeSceneChanged");
        yield return AccessTools.Field(sceneManagerType, "sceneLoaded");
        yield return AccessTools.Field(sceneManagerType, "sceneUnloaded");
    }
}