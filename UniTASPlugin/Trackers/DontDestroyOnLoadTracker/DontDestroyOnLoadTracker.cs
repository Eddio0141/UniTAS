using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace UniTASPlugin.Trackers.DontDestroyOnLoadTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class DontDestroyOnLoadTracker : IDontDestroyOnLoadTracker, IDontDestroyOnLoadInfo
{
    private readonly List<Object> _dontDestroyOnLoadObjects = new();

    public IEnumerable<Object> DontDestroyOnLoadObjects
    {
        get
        {
            _dontDestroyOnLoadObjects.RemoveAll(x => x == null);
            return _dontDestroyOnLoadObjects;
        }
    }

    public void DontDestroyOnLoad(Object obj)
    {
        if (DontDestroyOnLoadObjects.Any(x => x == obj)) return;

        Trace.Write($"DontDestroyOnLoad object, hash: {obj.GetHashCode()}");
        _dontDestroyOnLoadObjects.Add(obj);
    }
}