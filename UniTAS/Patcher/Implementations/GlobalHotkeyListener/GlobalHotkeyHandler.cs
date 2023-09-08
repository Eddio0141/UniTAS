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
        if (_hotkeys.Any(x => x.Bind.Key == config.Bind.Key))
        {
            throw new GlobalBindAlreadyExistsException(config);
        }
        
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