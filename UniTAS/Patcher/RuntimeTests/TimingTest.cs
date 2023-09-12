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
    private readonly IUpdateInvokeOffset _updateInvokeOffset;

    public TimingTest(IPatchReverseInvoker patchReverseInvoker, IUpdateInvokeOffset updateInvokeOffset)
    {
        _patchReverseInvoker = patchReverseInvoker;
        _updateInvokeOffset = updateInvokeOffset;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> UpdateInvokeOffsetTest()
    {
        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(_patchReverseInvoker.Invoke(() => Time.time) % Time.fixedDeltaTime,
            _updateInvokeOffset.Offset);
    }
}