using System;

namespace UniTAS.Plugin.Models.RuntimeTest;

public class TestResult
{
    public string TestName { get; }
    public bool Passed { get; }
    public Exception Exception { get; }

    public TestResult(string testName, bool passed, Exception exception = null)
    {
        TestName = testName;
        Passed = passed;
        Exception = exception;
    }
}