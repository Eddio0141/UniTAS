using UnityEngine;

namespace UniTAS.Patcher.Shared;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    static UpdateInvokeOffset()
    {
        MonoBehaviourEvents.OnUpdate += UpdateOffset;
    }

    private static void UpdateOffset()
    {
        Offset += Time.deltaTime;
        var fixedDeltaTime = Time.fixedDeltaTime;
        if (Offset > fixedDeltaTime)
            Offset -= fixedDeltaTime;
    }
}