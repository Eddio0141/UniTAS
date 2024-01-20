using UniTAS.Patcher.Models.UnitySafeWrappers;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface ICursorWrapper
{
    bool Visible { get; set; }
    CursorLockMode LockState { get; set; }
}