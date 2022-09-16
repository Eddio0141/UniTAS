using System;

namespace Core.UnityHook.Helpers;

public abstract class Base
{
    internal static Type ObjType { get; set; }

    public virtual void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        InitByUnityVersion(objType, version);
    }

    protected abstract void InitByUnityVersion(Type objType, UnityVersion version);
}