using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.RuntimeTest;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.RuntimeTest;

namespace UniTAS.Patcher.Implementations.RuntimeTest;

[Singleton]
public class RuntimeTestAndLog : IRuntimeTestAndLog
{
    private readonly IRuntimeTestProcessor _testProcessor;
    private readonly ILogger _logger;
    private bool _isTesting;

    public RuntimeTestAndLog(IRuntimeTestProcessor testProcessor, ILogger logger)
    {
        _testProcessor = testProcessor;
        _logger = logger;

        _testProcessor.OnDiscoveredTests += DiscoveredTests;
        _testProcessor.OnTestRun += Testing;
    }

    private void DiscoveredTests(int count)
    {
        _logger.LogInfo($"Discovered {count} runtime tests");
    }

    private void Testing(string name)
    {
        _logger.LogInfo($"Running runtime test: {name}");
    }

    public void Test()
    {
        if (_isTesting)
        {
            _logger.LogError("Cannot run test while another test is running");
            return;
        }

        _testProcessor.OnTestEnd += TestLog;
        _testProcessor.Test<RuntimeTestAndLog>();
        _isTesting = true;
    }

    private void TestLog(List<TestResult> results)
    {
        var builder = new StringBuilder();

        builder.Append($"Runtime tests | {results.Count} total | ");

        var passedCount = results.Count(x => x.Passed);
        var failedCount = results.Count(x => !x.Passed && !x.Skipped);
        var skippedCount = results.Count(x => x.Skipped);

        if (passedCount > 0)
        {
            builder.Append($"{results.Count(x => x.Passed)} passed");
        }

        if (passedCount > 0 && failedCount > 0)
        {
            builder.Append(" | ");
        }

        if (failedCount > 0)
        {
            builder.Append($"{results.Count(x => !x.Passed)} failed");
        }

        if ((passedCount > 0 || failedCount > 0) && skippedCount > 0)
        {
            builder.Append(" | ");
        }

        if (skippedCount > 0)
        {
            builder.Append($"{results.Count(x => x.Skipped)} skipped");
        }

        builder.AppendLine();

        results.OrderBy(x => x.Passed).ThenBy(x => x.TestName).ToList().ForEach(x =>
        {
            builder.AppendLine(
                "------------------------------------------------------------------------------------------------------------------------");

            builder.AppendLine($"{x.TestName} " + (x.Skipped ? "Skipped" : x.Passed ? "Passed" : "Failed"));

            if (x.Exception != null)
            {
                builder.AppendLine("\nException:");
                builder.AppendLine(x.Exception.ToString());
            }
        });

        builder.AppendLine(
            "------------------------------------------------------------------------------------------------------------------------");

        _logger.LogInfo($"\n{builder}");

        _testProcessor.OnTestEnd -= TestLog;
        _isTesting = false;
    }
}