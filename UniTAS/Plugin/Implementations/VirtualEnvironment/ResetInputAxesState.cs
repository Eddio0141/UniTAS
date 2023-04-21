using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment;

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