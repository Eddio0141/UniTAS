using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

public class Object : Base<Object>
{
    static MethodBase getInstanceID;
    static MethodBase findObjectsOfType;
    static MethodBase destroy;
    static MethodBase dontDestroyOnLoad;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                getInstanceID = objType.GetMethod("GetInstanceID", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null);
                findObjectsOfType = objType.GetMethod("FindObjectsOfType", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(Type) }, null);
                destroy = objType.GetMethod("Destroy", BindingFlags.Public | BindingFlags.Static, null, new Type[] { ObjType }, null);
                dontDestroyOnLoad = objType.GetMethod("DontDestroyOnLoad", BindingFlags.Public | BindingFlags.Static, null, new Type[] { ObjType }, null);
                break;
        }
    }

    public static int GetInstanceID(Args args)
    {
        return (int)getInstanceID.Invoke(args.Instance, args.Arguments);
    }

    public static object[] FindObjectsOfType(Args args)
    {
        return findObjectsOfType.Invoke(null, args.Arguments) as object[];
    }

    public static void Destroy(Args args)
    {
        destroy.Invoke(null, args.Arguments);
    }

    public static void DontDestroyOnLoad(Args args)
    {
        dontDestroyOnLoad.Invoke(null, args.Arguments);
    }
}
