using System;
using System.Collections.Generic;

namespace UniTASPlugin.GameObjectTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class ObjectTracker : IObjectTracker
{
    private readonly List<ObjectTrackingStatus> _trackedObjects = new();

    public void NewObject(object obj)
    {
        var hash = obj.GetHashCode();
        if (_trackedObjects.Exists(x => x.Hash == hash))
        {
            throw new InvalidOperationException("Object already tracked");
        }

        _trackedObjects.Add(new(obj));
        OnNewObject?.Invoke(hash);
    }

    public void DestroyObject(object obj)
    {
        var hash = obj.GetHashCode();
        var status = _trackedObjects.Find(x => x.Hash == hash);
        if (status == null)
        {
            throw new InvalidOperationException("Object not tracked");
        }

        _trackedObjects.Remove(status);
        OnDestroyObject?.Invoke(hash);
    }

    public event Action<int> OnNewObject;
    public event Action<int> OnDestroyObject;

    private class ObjectTrackingStatus
    {
        public int Hash { get; }

        public ObjectTrackingStatus(object obj)
        {
            Hash = obj.GetHashCode();
        }
    }
}