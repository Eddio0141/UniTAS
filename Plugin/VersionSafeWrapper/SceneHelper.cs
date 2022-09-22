using HarmonyLib;
using System;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class SceneHelper
{
    static Type SceneManager()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");
    }

    public static void LoadScene(int buildIndex)
    {
        var sceneManager = SceneManager();
        if (sceneManager != null)
        {
            var loadScene = AccessTools.Method(sceneManager, "LoadScene", new Type[] { typeof(int) });
            if (loadScene != null)
            {
                loadScene.Invoke(null, new object[] { buildIndex });
                return;
            }
        }
        throw new Exception("Failed to load scene, TODO add fallback");
    }
}
