using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.NoRefresh;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Patcher.Models.GUI;

public class WindowDependencies(
    IUpdateEvents updateEvents,
    IPatchReverseInvoker patchReverseInvoker,
    IConfig config,
    IToolBar toolBar,
    INoRefresh noRefresh,
    ITextureWrapper textureWrapper,
    IUnityInputWrapper unityInputWrapper)
{
    public IUpdateEvents UpdateEvents { get; } = updateEvents;
    public IPatchReverseInvoker PatchReverseInvoker { get; } = patchReverseInvoker;
    public IConfig Config { get; } = config;
    public INoRefresh NoRefresh { get; } = noRefresh;
    public IToolBar ToolBar { get; } = toolBar;
    public ITextureWrapper TextureWrapper { get; } = textureWrapper;
    public IUnityInputWrapper UnityInputWrapper { get; } = unityInputWrapper;
}