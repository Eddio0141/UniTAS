using UniTAS.Patcher.Interfaces.Invoker;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    [InvokeOnUnityInit]
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