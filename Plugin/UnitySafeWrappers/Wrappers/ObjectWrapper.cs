using System;
using System.Collections.Generic;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using Object = UnityEngine.Object;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

// ReSharper disable once ClassNeverInstantiated.Global
public class ObjectWrapper : IObjectWrapper
{
    public Type ObjectType => typeof(Object);

    public void DestroyImmediate(object obj)
    {
        Object.DestroyImmediate((Object)obj);
    }

    public void Destroy(object obj)
    {
        Object.Destroy((Object)obj);
    }

    public IEnumerable<object> FindObjectsOfType(Type type)
    {
        return Object.FindObjectsOfType(type);
    }
}