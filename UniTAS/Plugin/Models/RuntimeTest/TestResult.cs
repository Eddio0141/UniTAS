using System;

namespace UniTAS.Plugin.Models.RuntimeTest;

public class TestResult
{
    public string TestName { get; }
    public bool Passed { get; }
    public bool Skipped { get; }
    public Exception Exception { get; }

    public TestResult(string testName, bool passed, Exception exception = null)
    {
        TestName = testName;
        Passed = passed;
        Exception = exception;
    }

    // in case it was skipped
    public TestResult(string testName)
    {
        TestName = testName;
        Skipped = true;
    }
}