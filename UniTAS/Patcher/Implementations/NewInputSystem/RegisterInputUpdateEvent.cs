using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.StaticServices;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class RegisterInputUpdateEvent : IOnGameRestart
{
    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        InputSystemEvents.Init();
    }
}