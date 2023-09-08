using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Exceptions.GlobalHotkeyListener;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Models.GlobalHotkeyListener;

namespace UniTAS.Patcher.Implementations.GlobalHotkeyListener;

[Singleton]
[ExcludeRegisterIfTesting]
public class GlobalHotkeyHandler : IGlobalHotkey, IOnUpdateUnconditional
{
    private readonly List<GlobalHotkey> _hotkeys = new();

    public void AddGlobalHotkey(GlobalHotkey config)
    {
        var sameKey = _hotkeys.FirstOrDefault(x => x.Bind.Key == config.Bind.Key);
        if (sameKey != null && sameKey.Bind.Name != config.Bind.Name)
        {
            throw new GlobalBindAlreadyExistsException(config);
        }

        if (sameKey?.Bind?.Name == config.Bind.Name) return;

        _hotkeys.Add(config);
    }

    public void UpdateUnconditional()
    {
        foreach (var hotkey in _hotkeys)
        {
            if (hotkey.Bind.IsPressed())
            {
                hotkey.Callback();
            }
        }
    }
}