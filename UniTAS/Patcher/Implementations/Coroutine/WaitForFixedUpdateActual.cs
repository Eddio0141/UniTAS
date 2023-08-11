using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services.EventSubscribers;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForFixedUpdateActual : CoroutineWait
{
    private IUpdateEvents _updateEvents;

    private void LastUpdateActual()
    {
        RunNext();
        _updateEvents.OnFixedUpdateActual -= LastUpdateActual;
    }

    protected override void HandleWait()
    {
        _updateEvents = Container.GetInstance<IUpdateEvents>();
        _updateEvents.OnFixedUpdateActual += LastUpdateActual;
    }
}