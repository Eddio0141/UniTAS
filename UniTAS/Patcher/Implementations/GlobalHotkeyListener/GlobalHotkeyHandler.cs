using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.GlobalHotkeyListener;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Implementations.GlobalHotkeyListener;

[Singleton]
public class GlobalHotkeyHandler : IGlobalHotkey, IOnUpdateUnconditional
{
    private readonly List<Models.GlobalHotkeyListener.GlobalHotkey> _hotkeys = new();

    public void AddGlobalHotkey(Bind bind, Action callback)
    {
        _hotkeys.Add(new(bind, callback));
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