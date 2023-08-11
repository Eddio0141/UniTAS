using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Utils;

namespace Patcher.Tests.RuntimeTests;

public class RuntimeTests
{
    [RuntimeTest]
    public void RuntimeTestMethod()
    {
    }

    [RuntimeTest]
    public void RuntimeTestFail()
    {
        throw new("Runtime test failed");
    }

    [RuntimeTest]
    public bool SkipTest()
    {
        return false;
    }

    [RuntimeTest]
    public Tuple<bool, IEnumerable<CoroutineWait>> SkipAndCoroutineTest()
    {
        return new(false, null!);
    }

    [RuntimeTest]
    public List<int> WrongReturnType()
    {
        return new();
    }

    [RuntimeTest]
    public Tuple<bool, IEnumerable<CoroutineWait>> CoroutineTest()
    {
        return new(true, CoroutineTestInner());
    }

    private static IEnumerable<CoroutineWait> CoroutineTestInner()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> CoroutineTest2()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> CoroutineTestFail()
    {
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        throw new("Coroutine test failed");
    }
}