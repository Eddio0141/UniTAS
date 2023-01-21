using System;

namespace UniTASPlugin.GameObjectTracker;

public interface IObjectTracker
{
    void NewObject(object obj);
    void DestroyObject(object obj);

    event Action<int> OnNewObject;
    event Action<int> OnDestroyObject;
}