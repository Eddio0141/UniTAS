using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton]
public class ResetInputAxesState : IResetInputAxesState, IOnFixedUpdateActual, IOnUpdateActual
{
    public bool IsResetInputAxesState { get; set; }

    public void UpdateActual()
    {
        if (!IsResetInputAxesState) return;
        IsResetInputAxesState = false;
    }

    public void FixedUpdateActual()
    {
        if (!IsResetInputAxesState) return;
        IsResetInputAxesState = false;
    }
}