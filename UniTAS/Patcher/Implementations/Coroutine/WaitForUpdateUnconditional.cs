using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForUpdateUnconditional : CoroutineWait
{
    private IUpdateEvents _updateEvents;

    private void UpdateUnconditional()
    {
        RunNext();
        _updateEvents.OnUpdateUnconditional -= UpdateUnconditional;
    }

    protected override void HandleWait()
    {
        _updateEvents = Container.GetInstance<IUpdateEvents>();
        _updateEvents.OnUpdateUnconditional += UpdateUnconditional;
    }
}