namespace UniTASPlugin;

public class ReverseInvokerFactory : IReverseInvokerService
{
    private static readonly PatchReverseInvoker patchReverseInvoker = new();

    public PatchReverseInvoker GetReverseInvoker()
    {
        return patchReverseInvoker;
    }
}