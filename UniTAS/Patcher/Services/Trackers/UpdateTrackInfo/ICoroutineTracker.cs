using System.Collections;
using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface ICoroutineTracker
{
    void NewCoroutine(MonoBehaviour instance, IEnumerator routine);
    void NewCoroutine(MonoBehaviour instance, string methodName, object value);
}