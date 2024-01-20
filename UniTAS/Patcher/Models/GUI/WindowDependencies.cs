using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Models.GUI;

public class WindowDependencies(IUpdateEvents updateEvents, IPatchReverseInvoker patchReverseInvoker)
{
    public IUpdateEvents UpdateEvents { get; } = updateEvents;
    public IPatchReverseInvoker PatchReverseInvoker { get; } = patchReverseInvoker;
}