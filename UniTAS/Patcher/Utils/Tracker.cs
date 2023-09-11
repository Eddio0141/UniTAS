using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace UniTAS.Patcher.Utils;

public static class Tracker
{
    private static readonly Type Object = AccessTools.TypeByName("UnityEngine.Object");

    private static readonly MethodBase ObjectEquals =
        AccessTools.Method(Object, nameof(object.Equals), new[] { Object });

    public static readonly List<object> DontDestroyGameObjects = new();

    /// <summary>
    /// Contains all DontDestroyOnLoad root game objects.
    /// </summary>
    public static List<object> DontDestroyOnLoadRootObjects
    {
        get
        {
            // filter out destroyed objects
            DontDestroyGameObjects.RemoveAll(obj => (bool)ObjectEquals.Invoke(obj, new object[] { null }));
            return DontDestroyGameObjects;
        }
    }

    /// <summary>
    /// Contains the order in which static constructors were invoked.
    /// </summary>
    public static readonly List<Type> StaticCtorInvokeOrder = new();

    /// <summary>
    /// Contains all static fields so far found.
    /// </summary>
    public static readonly List<FieldInfo> StaticFields = new();
}