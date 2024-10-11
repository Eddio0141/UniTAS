using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface IWindowEnv
{
    IResolutionWrapper CurrentResolution { get; set; }
    bool FullScreen { get; set; }
    FullScreenModeWrap FullScreenMode { get; set; }
    IResolutionWrapper[] ExtraSupportedResolutions { get; set; }
}