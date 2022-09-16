using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

#pragma warning disable IDE1006

public class Cursor : Base
{
    static MethodInfo visibleGetter;
    static MethodInfo visibleSetter;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                visibleGetter = objType.GetMethod("visible", BindingFlags.GetField);
                visibleSetter = objType.GetMethod("visible", BindingFlags.SetField);
                break;
        }
    }

    public static bool visible
    {
        get => (bool)visibleGetter.Invoke(null, new object[] { });
        set => visibleSetter.Invoke(null, new object[] { value });
    }
}