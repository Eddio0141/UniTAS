using System.Collections.Generic;
using UnityEngine;

namespace UniTAS.Patcher.Runtime;

public static class Tracker
{
    internal static readonly List<GameObject> DontDestroyGameObjects = new();

    /// <summary>
    /// Contains all DontDestroyOnLoad root game objects.
    /// </summary>
    public static List<GameObject> DontDestroyOnLoadRootObjects
    {
        get
        {
            // filter out destroyed objects
            DontDestroyGameObjects.RemoveAll(obj => obj == null);
            return DontDestroyGameObjects;
        }
    }


    // for now we don't need this, but it's here if we need it

    // internal static readonly List<Object> DontDestroyObjects = new();

    /*
    public static List<Object> DontDestroyOnLoadObjects
    {
        get
        {
            // filter out destroyed objects
            DontDestroyObjects.RemoveAll(obj => obj == null);
            return DontDestroyObjects;
        }
    }
    */
}