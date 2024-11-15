using UniTAS.Patcher.Implementations.UnitySafeWrappers;

namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface IWindowEnv
{
    ResolutionWrapper CurrentResolution { get; set; }
    bool FullScreen { get; set; }
    FullScreenModeWrap FullScreenMode { get; set; }
    ResolutionWrapper[] ExtraSupportedResolutions { get; set; }
}