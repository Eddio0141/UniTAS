namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAsyncOperationIsInvokingOnComplete
{
    bool IsInvokingOnComplete { get; }
}