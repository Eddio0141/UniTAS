namespace UniTASPlugin.GameObjectTracker;

public interface IObjectTracker
{
    void NewObject(object obj);
    void DestroyObject(object obj);
}