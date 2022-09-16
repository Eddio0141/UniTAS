using System;

namespace Core.UnityHelpers.Types;

public abstract class Base
{
    internal static Type ObjType { get; set; }
    protected static UnityVersion Version { get; set; }

    public virtual void Init(Type objType, UnityVersion version)
    {
        ObjType = objType;
        Version = version;
    }
}