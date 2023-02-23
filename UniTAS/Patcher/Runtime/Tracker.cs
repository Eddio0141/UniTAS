using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher.Runtime;

public static class Tracker
{
    internal static readonly List<Object> DontDestroyObjects = new();

    public static List<Object> DontDestroyOnLoadObjects
    {
        get
        {
            // filter out destroyed objects
            DontDestroyObjects.RemoveAll(obj => obj == null);
            return DontDestroyObjects;
        }
    }
}