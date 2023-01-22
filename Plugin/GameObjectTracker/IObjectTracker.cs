using System;

namespace UniTASPlugin.GameObjectTracker;

public interface IObjectTracker
{
    void NewObject(object obj);
    void DestroyObject(object obj);
    void DestroyObjects(int sceneIndex);

    void SubscribeToNewObject(Action<int> action);
    void SubscribeToDestroyObject(Action<int> action);

    void UnsubscribeFromNewObject(Action<int> action);
    void UnsubscribeFromDestroyObject(Action<int> action);
}