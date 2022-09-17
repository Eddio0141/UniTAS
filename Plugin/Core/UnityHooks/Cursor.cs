using Core.UnityHooks.Helpers;
using System;
using System.Reflection;

namespace Core.UnityHooks;

#pragma warning disable IDE1006

internal class Cursor : Base<Cursor>
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
                var visible = objType.GetProperty("visible", BindingFlags.Public | BindingFlags.Static);
                visibleGetter = visible.GetGetMethod();
                visibleSetter = visible.GetSetMethod();
                var lockState = objType.GetProperty("lockState", BindingFlags.Public | BindingFlags.Static);
                lockStateGetter = lockState.GetGetMethod();
                lockStateSetter = lockState.GetSetMethod();
                break;
            case UnityVersion.v2018_4_25:
                visible = objType.GetProperty("visible", BindingFlags.Public | BindingFlags.Static);
                visibleGetter = visible.GetGetMethod();
                visibleSetter = visible.GetSetMethod();
                lockState = objType.GetProperty("lockState", BindingFlags.Public | BindingFlags.Static);
                lockStateGetter = lockState.GetGetMethod();
                lockStateSetter = lockState.GetSetMethod();
                break;
        }
    }

    internal static bool visible
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