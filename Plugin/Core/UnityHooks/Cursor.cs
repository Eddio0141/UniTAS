using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

#pragma warning disable IDE1006

public class Cursor : Base<Cursor>
{
    static MethodInfo visibleGetter;
    static MethodInfo visibleSetter;
    static MethodInfo lockStateGetter;
    static MethodInfo lockStateSetter;

    protected override void InitByUnityVersion(Type objType, UnityVersion version)
    {
        switch (version)
        {
            case UnityVersion.v2021_2_14:
                visibleGetter = objType.GetMethod("visible", BindingFlags.GetField);
                visibleSetter = objType.GetMethod("visible", BindingFlags.SetField);
                lockStateGetter = objType.GetMethod("lockState", BindingFlags.GetField);
                lockStateSetter = objType.GetMethod("lockState", BindingFlags.SetField);
                break;
        }
    }

    public static bool visible
    {
        get => (bool)visibleGetter.Invoke(null, new object[] { });
        set => visibleSetter.Invoke(null, new object[] { value });
    }

    internal static CursorLockModeType lockState
    {
        get => CursorLockMode.From(lockStateGetter.Invoke(null, new object[] { }));
        set => lockStateSetter.Invoke(null, new object[] { CursorLockMode.To(value) });
    }
}