using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers;

public interface ICoroutineRunningObjectsTracker
{
    void NewCoroutine(MonoBehaviour instance);
}