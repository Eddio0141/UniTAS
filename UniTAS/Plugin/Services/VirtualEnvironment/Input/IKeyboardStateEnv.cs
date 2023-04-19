using System.Collections.Immutable;
using UniTAS.Plugin.Models.VirtualEnvironment;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IKeyboardStateEnv
{
    ImmutableList<Key> Keys { get; }
    ImmutableList<Key> KeysDown { get; }
    ImmutableList<Key> KeysUp { get; }
    void Hold(Key key);
    void Release(Key key);
    void Clear();
}