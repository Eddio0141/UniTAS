using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Models.Invoker;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    [InvokeOnUnityInit(Priority = InvokerPriority.UpdateInvokeOffset)]
    public static void Init()
    {
        MonoBehaviourEvents.OnUpdateUnconditional += UpdateUnconditionalOffset;
    }

    private static void UpdateUnconditionalOffset()
    {
        Offset += Time.deltaTime;
        var fixedDeltaTime = Time.fixedDeltaTime;
        if (Offset > fixedDeltaTime)
            Offset -= fixedDeltaTime;
    }
}