using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Models.Movie;

public class WindowState(IResolutionWrapper currentResolution, IResolutionWrapper[] extraResolutions)
{
    public void SetWindowEnv(IWindowEnv windowEnv)
    {
        windowEnv.CurrentResolution = CurrentResolution;
        windowEnv.ExtraSupportedResolutions = ExtraResolutions;
    }

    public IResolutionWrapper CurrentResolution { get; } = currentResolution;
    public IResolutionWrapper[] ExtraResolutions { get; } = extraResolutions;
}