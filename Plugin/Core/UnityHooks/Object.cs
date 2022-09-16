using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

internal class Object : Base<Object>
{
    static MethodBase getInstanceID;
    static MethodBase findObjectsOfType__Type;
    static MethodBase destroy__Object;
    static MethodBase dontDestroyOnLoad;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                getInstanceID = objType.GetMethod("GetInstanceID", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null);
                findObjectsOfType__Type = objType.GetMethod("FindObjectsOfType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type) }, null);
                destroy__Object = objType.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static, null, new Type[] { ObjType }, null);
                dontDestroyOnLoad = objType.GetMethod("DontDestroyOnLoad", BindingFlags.Public | BindingFlags.Static, null, new Type[] { ObjType }, null);
                break;
        }
    }

    internal static int GetInstanceID(object instance)
    {
        return (int)getInstanceID.Invoke(instance, null);
    }

    internal static object[] FindObjectsOfType(Type type)
    {
        return findObjectsOfType__Type.Invoke(null, new object[] { type }) as object[];
    }

    internal static void Destroy(object obj)
    {
        destroy__Object.Invoke(null, new object[] { obj });
    }

    internal static void DontDestroyOnLoad(object target)
    {
        dontDestroyOnLoad.Invoke(null, new object[] { target });
    }
}
