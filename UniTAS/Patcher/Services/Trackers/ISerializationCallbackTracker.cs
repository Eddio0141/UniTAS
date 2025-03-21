namespace UniTAS.Patcher.Services.Trackers;

public interface ISerializationCallbackTracker
{
    bool OnAfterDeserializeInvoke(object instance);
}