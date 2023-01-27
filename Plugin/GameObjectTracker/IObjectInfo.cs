using System;

namespace UniTASPlugin.GameObjectTracker;

public interface IObjectInfo
{
    // void SubscribeToNewObject(Action<int> action);
    void SubscribeToDestroyObject(Action<int> action);
}