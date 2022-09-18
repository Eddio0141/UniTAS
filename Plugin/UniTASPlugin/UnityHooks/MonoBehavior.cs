using System;
using System.Reflection;

namespace UniTASPlugin.UnityHooks;

internal class MonoBehavior : Base<MonoBehavior>
{
    static MethodInfo stopAllCoroutines;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        stopAllCoroutines = objType.GetMethod("StopAllCoroutines", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
    }

    internal static void StopAllCoroutines(object instance)
    {
        stopAllCoroutines.Invoke(instance, null);
    }
}
