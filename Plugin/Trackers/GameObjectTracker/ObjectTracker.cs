using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniTASPlugin.Trackers.SceneTracker;

namespace UniTASPlugin.Trackers.GameObjectTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class ObjectTracker : IObjectTracker, IObjectInfo
{
    private readonly List<ObjectTrackingStatus> _trackedObjects = new();

    public ObjectTracker(ILoadedSceneInfo loadedSceneInfo)
    {
        loadedSceneInfo.SubscribeOnSceneLoad(SceneLoadCallback);
        loadedSceneInfo.SubscribeOnSceneUnload(SceneUnloadCallback);
    }

    private void SceneLoadCallback(SceneLoadInfo loadInfo)
    {
        if (loadInfo.Additive) return;

        _trackedObjects.RemoveAll(x => !x.DontDestroyOnLoad);
    }

    private void SceneUnloadCallback(SceneInfo sceneInfo)
    {
        _trackedObjects.RemoveAll(x => !x.DontDestroyOnLoad && sceneInfo.SceneIndex == x.SceneInfo.SceneIndex);
    }

    public void NewObject(object obj)
    {
        var hash = obj.GetHashCode();
        Trace.Write($"New object, hash: {hash}");
        if (_trackedObjects.Exists(x => x.Hash == hash))
        {
            throw new InvalidOperationException("Object already tracked");
        }

        _trackedObjects.Add(new(obj));
        // OnNewObject?.Invoke(hash);
    }

    public void DestroyObject(object obj)
    {
        var hash = obj.GetHashCode();
        Trace.Write($"Destroy object, hash: {hash}");
        var status = _trackedObjects.Find(x => x.Hash == hash);
        if (status == null)
        {
            throw new InvalidOperationException("Object not tracked");
        }

        _trackedObjects.Remove(status);
        OnDestroyObject?.Invoke(hash);
    }

    // private event Action<int> OnNewObject;
    private event Action<int> OnDestroyObject;

    // public void SubscribeToNewObject(Action<int> action)
    // {
    //     OnNewObject += action;
    // }

    public void SubscribeToDestroyObject(Action<int> action)
    {
        OnDestroyObject += action;
    }

    private class ObjectTrackingStatus
    {
        public int Hash { get; }
        public SceneInfo SceneInfo { get; }
        public bool DontDestroyOnLoad { get; set; }

        public ObjectTrackingStatus(object obj)
        {
            Hash = obj.GetHashCode();
        }
    }
}