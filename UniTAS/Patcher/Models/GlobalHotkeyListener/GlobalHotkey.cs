using System;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Models.GlobalHotkeyListener;

public class GlobalHotkey
{
    public Bind Bind { get; }
    public Action Callback { get; }

    public GlobalHotkey(Bind bind, Action callback)
    {
        Bind = bind;
        Callback = callback;
    }
}