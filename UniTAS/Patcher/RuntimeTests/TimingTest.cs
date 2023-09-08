using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

public class TimingTest
{
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public TimingTest(IPatchReverseInvoker patchReverseInvoker)
    {
        _patchReverseInvoker = patchReverseInvoker;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> UpdateInvokeOffsetTest()
    {
        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(_patchReverseInvoker.Invoke(() => Time.time) % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset);
    }
}