using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncOperationTracker
{
    bool ManagedInstance(AsyncOperation asyncOperation);
}