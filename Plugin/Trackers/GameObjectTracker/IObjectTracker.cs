namespace UniTASPlugin.Trackers.GameObjectTracker;

public interface IObjectTracker
{
    void NewObject(object obj);
    void DestroyObject(object obj);
}