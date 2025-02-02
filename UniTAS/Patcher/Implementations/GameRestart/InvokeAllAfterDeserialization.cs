using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.ManualServices.Trackers;

namespace UniTAS.Patcher.Implementations.GameRestart;

[Singleton]
public class InvokeAllAfterDeserialization : IOnGameRestart
{
    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        var bench = Bench.Measure();
        SerializationCallbackTracker.InvokeAllAfterDeserialization();
        bench.Dispose();
    }
}