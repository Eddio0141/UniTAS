using System;

namespace Core.UnityHelpers;

public static class Main
{
    internal static Type Object;
    internal static Type MonoBehavior;

    public static void Init(Type object_, Type monoBehavior)
    {
        Object = object_;
        MonoBehavior = monoBehavior;
    }
}
