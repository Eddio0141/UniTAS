using System.Collections;

namespace UniTASPlugin.Trackers.MonoBehCoroutineEndOfFrameTracker;

public interface IEndOfFrameTracker
{
    void NewCoroutine(IEnumerator enumerator, object coroutine, object monoBehaviourInstance = null,
        string stringCoroutineMethodName = null);

    void MoveNextInvoke(IEnumerator enumerator);

    void CoroutineEnd(IEnumerator enumerator);
    void CoroutineEnd(object monoBehaviourInstance, string coroutineMethodName);
    void CoroutineEnd(object coroutine);
    void CoroutineEndAll(object monoBehaviourInstance);
}