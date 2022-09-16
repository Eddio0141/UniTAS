using System;

namespace Core.UnityHelpers;

public static class Main
{
    public static void Init(Func<Type, object[]> findObjectsOfType, Func<object, int> getInstanceID, Type monoBehavior, Type keyCode, Type object_)
    {
        Types.Init(monoBehavior, keyCode);
        Object.Init(getInstanceID, findObjectsOfType, object_);
    }
}
