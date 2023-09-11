using System;
using System.Collections.Generic;
using System.Reflection;

namespace UniTAS.Patcher.Implementations.Trackers;

public static class Tracker
{
    /// <summary>
    /// Contains the order in which static constructors were invoked.
    /// </summary>
    public static readonly List<Type> StaticCtorInvokeOrder = new();

    /// <summary>
    /// Contains all static fields so far found.
    /// </summary>
    public static readonly List<FieldInfo> StaticFields = new();
}