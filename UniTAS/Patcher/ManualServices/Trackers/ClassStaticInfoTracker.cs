using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace UniTAS.Patcher.ManualServices.Trackers;

public class ClassStaticInfoTracker
{
    private readonly List<Type> _staticCtorInvokeOrder = new();
    private readonly List<FieldInfo> _staticFields = new();

    private static ClassStaticInfoTracker _instance;

    private static ClassStaticInfoTracker Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public static void AddStaticCtorForTracking(Type type)
    {
        Instance._staticCtorInvokeOrder.Add(type);
    }

    public static void AddStaticFields(IEnumerable<FieldInfo> fields)
    {
        Instance._staticFields.AddRange(fields);
    }

    public static ReadOnlyCollection<Type> StaticCtorInvokeOrder => Instance._staticCtorInvokeOrder.AsReadOnly();
    public static ReadOnlyCollection<FieldInfo> StaticFields => Instance._staticFields.AsReadOnly();
}