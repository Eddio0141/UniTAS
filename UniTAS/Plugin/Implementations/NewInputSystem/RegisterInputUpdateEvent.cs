using System;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
public class RegisterInputUpdateEvent : IOnGameRestart
{
    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        InputSystemEvents.Init();
    }
}