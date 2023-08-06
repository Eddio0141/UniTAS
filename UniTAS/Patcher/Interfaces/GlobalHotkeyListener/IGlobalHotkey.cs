using System;
using UniTAS.Patcher.Models.Customization;

namespace UniTAS.Patcher.Interfaces.GlobalHotkeyListener;

public interface IGlobalHotkey
{
    void AddGlobalHotkey(Bind bind, Action callback);
}