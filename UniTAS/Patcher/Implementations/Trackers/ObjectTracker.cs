using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services.Trackers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Trackers;

[Singleton(timing: RegisterTiming.Entry)]
public class ObjectTracker : IObjectTracker
{
    private readonly List<Object> _dontDestroyGameObjects = new();

    public void DontDestroyOnLoadAddRoot(Object obj)
    {
        _dontDestroyGameObjects.Add(obj);
    }

    public IEnumerable<Object> DontDestroyOnLoadRootObjects
    {
        get
        {
            // filter out destroyed objects
            _dontDestroyGameObjects.RemoveAll(obj => obj == null);
            return _dontDestroyGameObjects;
        }
    }
}