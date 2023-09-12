using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Models.GUI;

public class WindowDependencies
{
    public IUpdateEvents UpdateEvents { get; }
    public IPatchReverseInvoker PatchReverseInvoker { get; }

    public WindowDependencies(IUpdateEvents updateEvents, IPatchReverseInvoker patchReverseInvoker)
    {
        UpdateEvents = updateEvents;
        PatchReverseInvoker = patchReverseInvoker;
    }
}