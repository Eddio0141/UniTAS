using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[method: MoonSharpHidden]
public class Key(IKeyboardStateEnvController kbController, IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    : EngineMethodClass
{
    public void Hold(string key)
    {
        kbController.Hold(key);
        axisStateEnvLegacySystem.KeyDown(key, JoyNum.AllJoysticks);
    }

    public void Release(string key)
    {
        kbController.Release(key);
        axisStateEnvLegacySystem.KeyUp(key, JoyNum.AllJoysticks);
    }

    public void Clear()
    {
        kbController.Clear();
    }
}