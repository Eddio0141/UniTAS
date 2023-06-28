using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Models.GUI;

public class WindowDependencies
{
    public IUpdateEvents UpdateEvents { get; }
    public IWindowManager WindowManager { get; }
    public IPatchReverseInvoker PatchReverseInvoker { get; }

    public WindowDependencies(IUpdateEvents updateEvents, IWindowManager windowManager,
        IPatchReverseInvoker patchReverseInvoker)
    {
        UpdateEvents = updateEvents;
        WindowManager = windowManager;
        PatchReverseInvoker = patchReverseInvoker;
    }
}