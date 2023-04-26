using UniTAS.Plugin.Models.VirtualEnvironment;

namespace UniTAS.Plugin.Services.VirtualEnvironment.Input;

public interface IKeyboardStateEnv
{
    void Hold(Key key);
    void Release(Key key);
    void Clear();
    bool IsKeyDown(Key key);
    bool IsKeyUp(Key key);
    bool IsKeyHeld(Key key);
    bool AnyKeyHeld { get; }
    bool AnyKeyDown { get; }
}