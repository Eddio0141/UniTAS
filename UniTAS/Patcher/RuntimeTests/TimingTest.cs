using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

public class TimingTest(IPatchReverseInvoker patchReverseInvoker, IUpdateInvokeOffset updateInvokeOffset)
{
    [RuntimeTest]
    public IEnumerator<CoroutineWait> UpdateInvokeOffsetTest()
    {
        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(patchReverseInvoker.Invoke(() => Time.time) % Time.fixedDeltaTime,
            updateInvokeOffset.Offset);
    }
}