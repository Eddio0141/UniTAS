using System;

namespace Core.UnityHooks.Helpers;

public abstract class Base<T>
{
    internal static Type ObjType { get; set; }

    internal virtual void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        InitByUnityVersion(objType, version);
    }

    protected abstract void InitByUnityVersion(Type objType, UnityVersion version);
}