using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncOperationIsInvokingOnComplete
{
    bool IsInvokingOnComplete(AsyncOperation asyncOperation, out bool wasInvoked);
}