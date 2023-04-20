using System.Collections.Generic;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IButtonStateEnv
{
    List<string> Buttons { get; }
    List<string> ButtonsDown { get; }
    List<string> ButtonsUp { get; }
    void Hold(string button);
    void Release(string button);
    void Clear();
}