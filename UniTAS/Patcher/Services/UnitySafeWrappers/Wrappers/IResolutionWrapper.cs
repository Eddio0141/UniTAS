using UniTAS.Patcher.Implementations.UnitySafeWrappers;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface IResolutionWrapper
{
    int Width { get; set; }
    int Height { get; set; }
    RefreshRateWrap RefreshRateWrap { get; set; }
}