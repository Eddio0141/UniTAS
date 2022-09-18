using System;
using System.Reflection;

namespace UniTASPlugin.UnityHooks;

internal class Scene : Base<Scene>
{
    static MethodInfo buildIndexGetter;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        buildIndexGetter = objType.GetProperty("buildIndex", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
    }

    internal static int buildIndex(object scene)
    {
        return (int)buildIndexGetter.Invoke(scene, new object[0]);
    }
}
