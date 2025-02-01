using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Services.UnityEvents;

public interface IInputEventInvoker
{
    void InputSystemChangeUpdate(InputSettings.UpdateMode updateMode);
    void InputSystemEventsInit();
}