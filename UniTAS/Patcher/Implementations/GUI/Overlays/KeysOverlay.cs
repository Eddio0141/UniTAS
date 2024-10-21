using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.GUI;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GUI.Overlays;

[Singleton]
[ExcludeRegisterIfTesting]
public class KeysOverlay(
    WindowDependencies windowDependencies,
    IInputSystemState inputSystemState,
    IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem,
    IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem)
    : BuiltInOverlay(windowDependencies, "Keys", showByDefault: true)
{
    protected override AnchoredOffset DefaultOffset { get; } = new(0, 0, 0, 120);

    protected override string Update()
    {
        if (inputSystemState.HasNewInputSystem)
        {
            return keyboardStateEnvNewSystem.HeldKeys.Select(x => x.Key.ToString()).Join();
        }

        if (inputSystemState.HasOldInputSystem)
        {
            return keyboardStateEnvLegacySystem.HeldKeys.Select(x => x.KeyCode.ToString()).Join();
        }

        return null;
    }
}