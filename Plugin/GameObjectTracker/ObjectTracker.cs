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

    private event Action<int> OnNewObject;
    private event Action<int> OnDestroyObject;

    public void SubscribeToNewObject(Action<int> action)
    {
        OnNewObject += action;
    }

    public void SubscribeToDestroyObject(Action<int> action)
    {
        OnDestroyObject += action;
    }

    public void UnsubscribeFromNewObject(Action<int> action)
    {
        OnNewObject -= action;
    }

    public void UnsubscribeFromDestroyObject(Action<int> action)
    {
        OnDestroyObject -= action;
    }

    private class ObjectTrackingStatus
    {
        public int Hash { get; }

        public ObjectTrackingStatus(object obj)
        {
            Hash = obj.GetHashCode();
        }
    }
}