using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

internal class SceneManager : Base<SceneManager>
{
    static MethodInfo loadScene__int;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        loadScene__int = objType.GetMethod("LoadScene", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(int) }, null);
    }

    internal static void LoadScene(int sceneBuildIndex)
    {
        loadScene__int.Invoke(null, new object[] { sceneBuildIndex });
    }
}