using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace UniTAS.Patcher.Runtime;

public static class Tracker
{
    private static readonly Type Object = AccessTools.TypeByName("UnityEngine.Object");

    private static readonly MethodBase ObjectEquals =
        AccessTools.Method(Object, nameof(object.Equals), new[] { Object });

    internal static readonly List<object> DontDestroyGameObjects = new();

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


    // for now we don't need this, but it's here if we need it

    // internal static readonly List<object> DontDestroyObjects = new();

    /*
    public static List<object> DontDestroyOnLoadObjects
    {
        get
        {
            // filter out destroyed objects
            DontDestroyObjects.RemoveAll(obj => obj == null);
            return DontDestroyObjects;
        }
    }
    */

    internal static readonly List<Type> StaticCtorInvokeOrder = new();

    /// <summary>
    /// Contains the order in which static constructors were invoked.
    /// </summary>
    public static List<Type> StaticCtorInvokeOrderList => StaticCtorInvokeOrder;

    /// <summary>
    /// If true, static constructors will not be invoked.
    /// </summary>
    public static bool StopStaticCtorExecution { get; set; }
}