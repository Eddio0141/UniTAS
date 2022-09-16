using System;

namespace Core.UnityHelpers;

internal class Object
{
    internal static Type Object_;
    
    static Func<object, int> getInstanceID;
    static Func<Type, object[]> findObjectsOfType;

    internal static int GetInstanceID(object obj)
    {
        return getInstanceID(obj);
    }

    internal static void Init(Func<object, int> getInstanceID, Func<Type, object[]> findObjectsOfType, Type object_)
    {
        Object.getInstanceID = getInstanceID;
        Object.findObjectsOfType = findObjectsOfType;
        Object_ = object_;
    }

    internal static object[] FindObjectsOfType(Type type)
    {
        return findObjectsOfType?.Invoke(type);
    }
}
