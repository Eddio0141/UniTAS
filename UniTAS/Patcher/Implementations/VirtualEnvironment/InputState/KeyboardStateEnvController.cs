using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class KeyboardStateEnvController(
    IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem,
    IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem,
    IInputSystemState inputSystemState)
    : IKeyboardStateEnvController
{
    public void Hold(string key, out string warningMsg)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);
        warningMsg = null;

        if (keyCode.HasValue)
        {
            keyboardStateEnvLegacySystem.Hold(keyCode.Value);
        }
        else if (inputSystemState.HasOldInputSystem)
        {
            warningMsg = "failed to find matching KeyCode, make sure it's a valid entry in UnityEngine.KeyCode";
        }

        if (newKey.HasValue)
        {
            keyboardStateEnvNewSystem.Hold(newKey.Value);
        }
        else if (inputSystemState.HasNewInputSystem)
        {
            if (warningMsg == null)
                warningMsg = "";
            else
                warningMsg += "\n";
            warningMsg += "failed to find matching keycode for new unity input system, COULD BE A BUG";
        }
    }

    public void Release(string key, out string warningMsg)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);
        warningMsg = null;

        if (keyCode.HasValue)
        {
            keyboardStateEnvLegacySystem.Release(keyCode.Value);
        }
        else if (inputSystemState.HasOldInputSystem)
        {
            warningMsg = "failed to find matching KeyCode, make sure it's a valid entry in UnityEngine.KeyCode";
        }

        if (newKey.HasValue)
        {
            keyboardStateEnvNewSystem.Release(newKey.Value);
        }
        else if (inputSystemState.HasNewInputSystem)
        {
            if (warningMsg == null)
                warningMsg = "";
            else
                warningMsg += "\n";
            warningMsg += "failed to find matching keycode for new unity input system, COULD BE A BUG";
        }
    }

    public void Clear()
    {
        keyboardStateEnvLegacySystem.Clear();
        keyboardStateEnvNewSystem.Clear();
    }
}