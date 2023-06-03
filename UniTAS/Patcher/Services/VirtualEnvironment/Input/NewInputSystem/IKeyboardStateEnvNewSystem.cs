using System.Collections.Generic;
using UniTAS.Patcher.Models.VirtualEnvironment;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

public interface IKeyboardStateEnvNewSystem
{
    void Hold(Key key);
    void Release(Key key);
    List<Key> HeldKeys { get; }
    void Clear();
}