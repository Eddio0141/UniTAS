using System.Collections.Generic;
using UniTAS.Plugin.Models.VirtualEnvironment;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IKeyboardStateEnvNewSystem
{
    void Hold(Key key);
    void Release(Key key);
    List<Key> HeldKeys { get; }
    void Clear();
}