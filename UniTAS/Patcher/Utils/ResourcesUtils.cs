using System.Linq;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class ResourcesUtils
{
    public static T[] FindObjectsOfTypeAll<T>() where T : Object
    {
        return Resources.FindObjectsOfTypeAll(typeof(T)).Select(x => x as T).ToArray();
    }
}