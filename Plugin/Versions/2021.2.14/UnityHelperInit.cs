using System;

internal static class UnityHelperInit
{
    internal static void Init(Func<Type, object[]> findObjectsOfType, Func<object, int> getInstanceID, Type monoBehavior, Type keyCode, Type object_)
    {
        Core.UnityHelpers.Main.Init(findObjectsOfType, getInstanceID, monoBehavior, keyCode, object_);
    }
}