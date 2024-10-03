using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface ICoroutineRunningObjectsTracker
{
    void NewCoroutine(MonoBehaviour instance);
}