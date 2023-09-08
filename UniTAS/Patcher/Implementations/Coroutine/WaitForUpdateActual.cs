using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services.EventSubscribers;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForUpdateActual : CoroutineWait
{
    private IUpdateEvents _updateEvents;
    
    private void UpdateActual()
    {
        RunNext();
        _updateEvents.OnUpdateActual -= UpdateActual;
    }

    protected override void HandleWait()
    {
        _updateEvents = Container.GetInstance<IUpdateEvents>();
        _updateEvents.OnUpdateActual += UpdateActual;
    }
}