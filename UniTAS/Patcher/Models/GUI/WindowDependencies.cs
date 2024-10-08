using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.NoRefresh;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Models.GUI;

public class WindowDependencies(
    IUpdateEvents updateEvents,
    IPatchReverseInvoker patchReverseInvoker,
    IConfig config,
    INoRefresh noRefresh)
{
    public IUpdateEvents UpdateEvents { get; } = updateEvents;
    public IPatchReverseInvoker PatchReverseInvoker { get; } = patchReverseInvoker;
    public IConfig Config { get; } = config;
    public INoRefresh NoRefresh { get; } = noRefresh;
}