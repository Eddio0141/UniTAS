using System;

namespace UniTASPlugin.UnityHooks.Helpers;

public abstract class Base<T>
{
    internal static Type ObjType { get; set; }

    internal virtual void Init(Type objType)
    {
        ObjType = objType;
        InitByUnityVersion(objType);
    }

    protected abstract void InitByUnityVersion(Type objType);
}