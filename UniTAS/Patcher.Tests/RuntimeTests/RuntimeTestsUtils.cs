namespace Patcher.Tests.RuntimeTests;

public static class RuntimeTestsUtils
{
    public static readonly string[] NormalTests =
    {
        nameof(RuntimeTests.RuntimeTestMethod),
        nameof(RuntimeTests.RuntimeTestFail),
        nameof(RuntimeTests.SkipTest),
        nameof(RuntimeTests.WrongReturnType)
    };

    public static readonly string[] CoroutineTests =
    {
        nameof(RuntimeTests.SkipAndCoroutineTest),
        nameof(RuntimeTests.CoroutineTest),
        nameof(RuntimeTests.CoroutineTest2),
        nameof(RuntimeTests.CoroutineTestFail)
    };

    private static readonly string[] FailTests =
    {
        nameof(RuntimeTests.RuntimeTestFail),
        nameof(RuntimeTests.CoroutineTestFail)
    };

    private static readonly string[] SkipTests =
    {
        nameof(RuntimeTests.SkipTest),
        nameof(RuntimeTests.SkipAndCoroutineTest)
    };

    public static readonly string[] SkippedCoroutineTests =
    {
        nameof(RuntimeTests.SkipAndCoroutineTest)
    };

    public static readonly string[] FailedCoroutineTests =
    {
        nameof(RuntimeTests.CoroutineTestFail)
    };

    public static int TotalCount { get; } = NormalTests.Length + CoroutineTests.Length;

    public static int FailCount { get; } = FailTests.Length;

    public static int SkipCount { get; } = SkipTests.Length;

    public static int PassCount { get; } = TotalCount - FailCount - SkipCount;
}