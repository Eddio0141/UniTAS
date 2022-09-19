using System;
using System.Reflection;
using UniTASPlugin.UnityHooks.Helpers;

namespace UniTASPlugin.UnityHooks;

internal class SceneManager : Base<SceneManager>
{
    static MethodInfo loadScene__int;
    static MethodInfo getActiveScene;

    protected override void InitByUnityVersion(Type objType)
    {
        loadScene__int = objType.GetMethod("LoadScene", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(int) }, null);
        getActiveScene = objType.GetMethod("GetActiveScene", BindingFlags.Public | BindingFlags.Static, null, new Type[0], null);
    }

    internal static void LoadScene(int sceneBuildIndex)
    {
        loadScene__int.Invoke(null, new object[] { sceneBuildIndex });
    }

    internal static object GetActiveScene()
    {
        return getActiveScene.Invoke(null, new object[0]);
    }
}