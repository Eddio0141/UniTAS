using UnityEngine;

namespace UniTAS.Patcher.StaticServices;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    // TODO make it attribute or something to forced rather than manually invoking
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