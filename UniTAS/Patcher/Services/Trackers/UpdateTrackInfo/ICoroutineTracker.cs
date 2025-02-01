using System.Collections;
using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface ICoroutineTracker
{
    void NewCoroutine(MonoBehaviour instance, IEnumerator routine);

    // `object` replacement for accessing this function in preload patcher
    void NewCoroutine(object instance, IEnumerator routine);
    void NewCoroutine(MonoBehaviour instance, string methodName, object value);
    bool CoroutineMoveNextPrefix(IEnumerator instance, ref bool result, ref bool state);
    void CoroutineMoveNextPostfix(IEnumerator instance, bool state);
    void CoroutineCurrentPostfix(IEnumerator instance, ref object result);

    bool HasEndOfFrameCoroutineThisFrame { get; }
}