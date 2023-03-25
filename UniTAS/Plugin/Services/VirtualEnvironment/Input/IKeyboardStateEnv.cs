using System.Collections.Immutable;
using UnityEngine;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

public interface IKeyboardStateEnv
{
    ImmutableList<KeyCode> Keys { get; }
    ImmutableList<KeyCode> KeysDown { get; }
    ImmutableList<KeyCode> KeysUp { get; }
    void Hold(KeyCode key);
    void Release(KeyCode key);
    void Clear();
}