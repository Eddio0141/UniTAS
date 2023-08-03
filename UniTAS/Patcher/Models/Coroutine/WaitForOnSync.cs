using UniTAS.Patcher.Interfaces.Coroutine;

namespace UniTAS.Patcher.Models.Coroutine;

public class WaitForOnSync : CoroutineWait
{
    public double InvokeOffset { get; }
    public uint FixedUpdateIndex { get; }

    public WaitForOnSync(double invokeOffset = 0, uint fixedUpdateIndex = 0)
    {
        InvokeOffset = invokeOffset;
        FixedUpdateIndex = fixedUpdateIndex;
    }
}