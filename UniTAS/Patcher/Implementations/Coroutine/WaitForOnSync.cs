using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Services;

namespace UniTAS.Patcher.Implementations.Coroutine;

public class WaitForOnSync : CoroutineWait
{
    private readonly double _invokeOffset;
    private readonly uint _fixedUpdateIndex;

    public WaitForOnSync(double invokeOffset = 0, uint fixedUpdateIndex = 0)
    {
        _invokeOffset = invokeOffset;
        _fixedUpdateIndex = fixedUpdateIndex;
    }

    protected override void HandleWait()
    {
        var sync = Container.GetInstance<ISyncFixedUpdateCycle>();
        sync.OnSync(RunNext, _invokeOffset, _fixedUpdateIndex);
    }
}