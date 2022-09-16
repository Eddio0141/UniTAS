using System;

namespace Core.UnityHelpers;

public static class Types
{
    public static Type MonoBehavior;
    public static Type KeyCode;

    internal static void Init(Type monoBehavior, Type keyCode)
    {
        MonoBehavior = monoBehavior;
        KeyCode = keyCode;
    }
}
