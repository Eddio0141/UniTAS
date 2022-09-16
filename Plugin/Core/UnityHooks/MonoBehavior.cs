using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

internal class MonoBehavior : Base<MonoBehavior>
{
    static MethodInfo stopAllCoroutines;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                stopAllCoroutines = objType.GetMethod("StopAllCoroutines", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { }, null);
                break;
        }
    }

    internal static void StopAllCoroutines(object instance)
    {
        stopAllCoroutines.Invoke(instance, null);
    }
}
