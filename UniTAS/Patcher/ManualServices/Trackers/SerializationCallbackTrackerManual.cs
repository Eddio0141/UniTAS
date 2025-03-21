using UniTAS.Patcher.Services.Trackers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.ManualServices.Trackers;

public static class SerializationCallbackTrackerManual
{
    private static readonly ISerializationCallbackTracker SerializationCallbackTracker =
        ContainerStarter.Kernel.GetInstance<ISerializationCallbackTracker>();

    // ReSharper disable once InconsistentNaming
    public static bool OnAfterDeserializeInvoke(object __instance) =>
        SerializationCallbackTracker.OnAfterDeserializeInvoke(__instance);
}