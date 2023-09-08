using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services.EventSubscribers;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForFixedUpdateUnconditional : CoroutineWait
{
    private IUpdateEvents _updateEvents;

    private void LastUpdateUnconditional()
    {
        RunNext();
        _updateEvents.OnFixedUpdateUnconditional -= LastUpdateUnconditional;
    }

    protected override void HandleWait()
    {
        _updateEvents = Container.GetInstance<IUpdateEvents>();
        _updateEvents.OnFixedUpdateUnconditional += LastUpdateUnconditional;
    }
}