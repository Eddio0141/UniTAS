using System;
using System.Reflection;
using HarmonyLib;

namespace UniTAS.Patcher.Shared;

public static class UpdateInvokeOffset
{
    public static double Offset { get; private set; }

    private static readonly Type Time = AccessTools.TypeByName("UnityEngine.Time");
    private static readonly MethodBase DeltaTime = AccessTools.PropertyGetter(Time, "deltaTime");
    private static readonly MethodBase FixedDeltaTime = AccessTools.PropertyGetter(Time, "fixedDeltaTime");

    static UpdateInvokeOffset()
    {
        MonoBehaviourEvents.OnUpdate += UpdateOffset;
    }

    private static void UpdateOffset()
    {
        Offset += (float)DeltaTime.Invoke(null, null);
        var fixedDeltaTime = (float)FixedDeltaTime.Invoke(null, null);
        if (Offset > fixedDeltaTime)
            Offset -= fixedDeltaTime;
    }
}