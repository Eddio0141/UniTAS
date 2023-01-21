using System.Collections;

namespace UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;

public interface IEndOfFrameTracker
{
    void NewCoroutine(IEnumerator coroutine, object monoBehaviourInstance = null,
        string stringCoroutineMethodName = null);

    void MoveNextInvoke(IEnumerator coroutine);
    void CoroutineEnd(IEnumerator coroutine);
    void CoroutineEnd(object monoBehaviourInstance, string coroutineMethodName);
}