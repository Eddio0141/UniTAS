using System.Collections.Immutable;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState.Input;

public interface IButtonStateEnv
{
    ImmutableList<string> Buttons { get; }
    ImmutableList<string> ButtonsDown { get; }
    ImmutableList<string> ButtonsUp { get; }
    void Hold(string button);
    void Release(string button);
    void Clear();
}