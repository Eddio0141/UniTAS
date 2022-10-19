using UniTASPlugin.UpdateHelper;

namespace UniTASPlugin.GameEnvironment.InnerState.Input;

public abstract class InputDeviceBase : IOnUpdate
{
    public abstract void Update(float deltaTime);
    public abstract void ResetState();
}