namespace Patcher.Tests.RuntimeTests;

public static class RuntimeTestsUtils
{
    private static readonly string[] NormalTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.GameRestartValid)}",
        $"RuntimeTests.{nameof(RuntimeTests.RuntimeTestMethod)}",
        $"RuntimeTests.{nameof(RuntimeTests.RuntimeTestFail)}",
        $"RuntimeTests.{nameof(RuntimeTests.SkipTest)}",
        $"RuntimeTests.{nameof(RuntimeTests.WrongReturnType)}"
    ];

    public static readonly string[] CoroutineTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.SkipAndCoroutineTest)}",
        $"RuntimeTests.{nameof(RuntimeTests.CoroutineTest)}",
        $"RuntimeTests.{nameof(RuntimeTests.CoroutineTest2)}",
        $"RuntimeTests.{nameof(RuntimeTests.CoroutineTestFail)}"
    ];

    private static readonly string[] FailTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.RuntimeTestFail)}",
        $"RuntimeTests.{nameof(RuntimeTests.CoroutineTestFail)}"
    ];

    private static readonly string[] SkipNormalTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.SkipTest)}"
    ];

    private static readonly string[] SkippedCoroutineTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.SkipAndCoroutineTest)}"
    ];

    private static readonly string[] FailedCoroutineTests =
    [
        $"RuntimeTests.{nameof(RuntimeTests.CoroutineTestFail)}"
    ];

    public static int NormalTestCount { get; } = NormalTests.Length;

    public static int SkippedCoroutineTestCount { get; } = SkippedCoroutineTests.Length;

    public static int FailedCoroutineTestCount { get; } = FailedCoroutineTests.Length;

    public static int TotalCount { get; } = NormalTestCount + CoroutineTests.Length;

    public static int FailCount { get; } = FailTests.Length;

    public static int SkipNormalTestCount { get; } = SkipNormalTests.Length;

    public static int PassCount { get; } = TotalCount - FailCount - SkipNormalTestCount;
}