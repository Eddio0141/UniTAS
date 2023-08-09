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

        _testProcessor.OnTestsFinish += TestLog;
        _testProcessor.Test<RuntimeTestAndLog>();
        _isTesting = true;
    }

    private void TestLog(TestResults results)
    {
        _logger.LogInfo($"\n{results}");

        _testProcessor.OnTestsFinish -= TestLog;
        _isTesting = false;
    }
}