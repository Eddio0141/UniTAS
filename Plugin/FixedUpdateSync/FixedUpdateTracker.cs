using System;
using System.Collections.Generic;
using UniTASPlugin.Interfaces.Update;

namespace UniTASPlugin.FixedUpdateSync;

// ReSharper disable once ClassNeverInstantiated.Global
public class FixedUpdateTracker : IOnUpdate, IOnFixedUpdate, ISyncFixedUpdate
{
    private int _fixedUpdateIndex = -1;
    private readonly List<Action> _onSyncCallbacks = new();

    public void Update(float deltaTime)
    {
        _fixedUpdateIndex = -1;
        foreach (var onSyncCallback in _onSyncCallbacks)
        {
            onSyncCallback.Invoke();
        }

        _onSyncCallbacks.Clear();
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        _fixedUpdateIndex++;
    }

    public void OnSync(Action callback)
    {
        // callback if we are in the same fixed update
        if (_fixedUpdateIndex == -1)
        {
            callback.Invoke();
        }
        else
        {
            _onSyncCallbacks.Add(callback);
        }
    }
}