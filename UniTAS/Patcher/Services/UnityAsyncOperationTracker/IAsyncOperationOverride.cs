using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncOperationOverride
{
    bool GetPriority(AsyncOperation op, out int priority);
    bool SetPriority(AsyncOperation op, int priority);

    /// <summary>
    /// Gets the progress
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="progress">Progress of the AsyncOperation if it is tracked by UniTAS</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool Progress(AsyncOperation asyncOperation, out float progress);

    /// <summary>
    /// Gets if done
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="isDone">If IsDone is true or false, USE THIS NOT THE RETURN VALUE</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool IsDone(AsyncOperation asyncOperation, out bool isDone);

    /// <summary>
    /// For when the operation is yielded
    /// </summary>
    /// <returns>If instance is tracked</returns>
    bool Yield(AsyncOperation asyncOperation);
}