using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Models.Invoker;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    private static bool _updated;

    [InvokeOnUnityInit(Priority = InvokerPriority.UpdateInvokeOffset)]
    public static void Init()
    {
        // to make sure this is called before any other update events, we register on both Update and InputUpdate
        MonoBehaviourEvents.UpdatesUnconditional.Add(UpdateOffset, (int)CallbackPriority.UpdateInvokeOffset);
        InputSystemEvents.InputUpdatesUnconditional.Add((_, _) => UpdateOffset(),
            (int)CallbackPriority.UpdateInvokeOffset);

        MonoBehaviourEvents.OnLastUpdateUnconditional += () => _updated = false;
    }

    private static void UpdateOffset()
    {
        if (_updated) return;
        _updated = true;

        Offset += Time.deltaTime;
        Offset %= Time.fixedDeltaTime;
        StaticLogger.Trace($"New update offset: {Offset}, dt: {Time.deltaTime}");
    }
}