using UnityEngine;

namespace UniTAS.Plugin.Utils;

public static class UnityObjUtils
{
    public static T FindObjectOfType<T>() where T : Object
    {
        return (T)Object.FindObjectOfType(typeof(T));
    }
}