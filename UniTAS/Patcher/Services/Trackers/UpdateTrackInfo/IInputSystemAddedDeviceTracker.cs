using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IInputSystemAddedDeviceTracker
{
    void AddDevice(InputDevice device);
}