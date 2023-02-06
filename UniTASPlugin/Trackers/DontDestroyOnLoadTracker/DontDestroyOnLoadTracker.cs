using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class DontDestroyOnLoadTracker : IDontDestroyOnLoadTracker, IDontDestroyOnLoadInfo
{
    private readonly List<object> _dontDestroyOnLoadObjects = new();

    public IEnumerable<object> DontDestroyOnLoadObjects
    {
        get
        {
            _dontDestroyOnLoadObjects.RemoveAll(x => (Object)x == null);
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