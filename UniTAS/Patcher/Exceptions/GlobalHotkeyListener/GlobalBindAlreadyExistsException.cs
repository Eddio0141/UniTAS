using System;
using UniTAS.Patcher.Models.GlobalHotkeyListener;

namespace UniTAS.Patcher.Exceptions.GlobalHotkeyListener;

public class GlobalBindAlreadyExistsException : Exception
{
    public GlobalBindAlreadyExistsException(GlobalHotkey config) : base(
        $"Global hotkey bind {config.Bind.Key} already exists")
    {
    }
}