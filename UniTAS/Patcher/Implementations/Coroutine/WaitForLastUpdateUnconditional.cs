using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForLastUpdateUnconditional : CoroutineWait
{
    private IUpdateEvents _updateEvents;

    private void LastUpdateUnconditional()
    {
        RunNext();
        _updateEvents.OnLastUpdateUnconditional -= LastUpdateUnconditional;
    }

    protected override void HandleWait()
    {
        _updateEvents = Container.GetInstance<IUpdateEvents>();
        _updateEvents.OnLastUpdateUnconditional += LastUpdateUnconditional;
    }
}