namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface IAsyncOperationIsInvokingOnComplete
{
    bool IsInvokingOnComplete { get; }
}