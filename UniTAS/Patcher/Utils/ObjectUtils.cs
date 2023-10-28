using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class ObjectUtils
{
    public static T[] FindObjectsOfType<T>()
    {
        return Object.FindObjectsOfType(typeof(T)) as T[];
    }
}