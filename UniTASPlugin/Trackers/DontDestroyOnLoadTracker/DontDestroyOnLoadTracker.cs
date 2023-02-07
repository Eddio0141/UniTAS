using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UnityEngine;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class DontDestroyOnLoadTracker : IDontDestroyOnLoadTracker, IDontDestroyOnLoadInfo
{
    private readonly List<object> _dontDestroyOnLoadObjects = new();

    private readonly IObjectWrapper _objectWrapper;

    public DontDestroyOnLoadTracker(IObjectWrapper objectWrapper)
    {
        _objectWrapper = objectWrapper;
    }

    public IEnumerable<object> DontDestroyOnLoadObjects
    {
        get
        {
            _dontDestroyOnLoadObjects.RemoveAll(x => _objectWrapper.IsInstanceNullOrDestroyed(x));
            return _dontDestroyOnLoadObjects;
        }
    }

    public void DontDestroyOnLoad(object obj)
    {
        if (obj is not Object unityObject)
        {
            throw new System.ArgumentException("Object is not a Unity object");
        }

        if (DontDestroyOnLoadObjects.Any(x => (Object)x == unityObject)) return;

        Trace.Write($"DontDestroyOnLoad object, hash: {unityObject.GetHashCode()}");
        _dontDestroyOnLoadObjects.Add(unityObject);
    }
}