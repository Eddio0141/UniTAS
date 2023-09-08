namespace Patcher.Tests.RuntimeTests;

public static class RuntimeTestsUtils
{
    private static readonly string[] NormalTests =
    {
        nameof(RuntimeTests.RuntimeTestMethod),
        nameof(RuntimeTests.RuntimeTestFail),
        nameof(RuntimeTests.SkipTest),
        nameof(RuntimeTests.WrongReturnType),
        nameof(RuntimeTests.GameRestartValid)
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

    private static readonly string[] SkipNormalTests =
    {
        nameof(RuntimeTests.SkipTest)
    };

    private static readonly string[] SkippedCoroutineTests =
    {
        nameof(RuntimeTests.SkipAndCoroutineTest)
    };

    private static readonly string[] FailedCoroutineTests =
    {
        nameof(RuntimeTests.CoroutineTestFail)
    };

    public static int NormalTestCount { get; } = NormalTests.Length;

    public static int SkippedCoroutineTestCount { get; } = SkippedCoroutineTests.Length;

    public static int FailedCoroutineTestCount { get; } = FailedCoroutineTests.Length;

    public static int TotalCount { get; } = NormalTestCount + CoroutineTests.Length;

    public static int FailCount { get; } = FailTests.Length;

    public static int SkipNormalTestCount { get; } = SkipNormalTests.Length;

    public static int PassCount { get; } = TotalCount - FailCount - SkipNormalTestCount;
}