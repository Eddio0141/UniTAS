using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton]
public class ResetInputAxesState : IResetInputAxesState, IOnPreUpdatesActual
{
    public bool IsResetInputAxesState { get; set; }

    public void PreUpdateActual()
    {
        if (!IsResetInputAxesState) return;
        IsResetInputAxesState = false;
    }
}