using System.Collections;

namespace UniTASPlugin.MonoBehCoroutineEndOfFrameTracker;

public interface IEndOfFrameTracker
{
    void NewCoroutine(IEnumerator coroutine);
    void MoveNextInvoke(IEnumerator coroutine);
    void CoroutineEnd(IEnumerator coroutine);
}