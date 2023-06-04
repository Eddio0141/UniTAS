using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.UnregisterEventsOnRestart;

namespace UniTAS.Patcher.Implementations.UnregisterEventsOnRestart;

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