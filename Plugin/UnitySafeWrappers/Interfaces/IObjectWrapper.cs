using System;
using System.Collections.Generic;

namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface IObjectWrapper
{
    Type ObjectType { get; }
    void DestroyImmediate(object obj);
    void Destroy(object obj);
    IEnumerable<object> FindObjectsOfType(Type type);
}