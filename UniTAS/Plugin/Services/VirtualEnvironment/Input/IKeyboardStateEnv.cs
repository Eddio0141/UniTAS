using System.Collections.Generic;
using UniTAS.Plugin.Models.VirtualEnvironment;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IKeyboardStateEnv
{
    List<Key> Keys { get; }
    List<Key> KeysDown { get; }
    List<Key> KeysUp { get; }
    void Hold(Key key);
    void Release(Key key);
    void Clear();
}