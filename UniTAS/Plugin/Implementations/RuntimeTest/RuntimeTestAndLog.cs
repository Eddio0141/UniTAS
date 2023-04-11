using System.Linq;
using System.Text;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.RuntimeTest;

namespace UniTAS.Plugin.Implementations.RuntimeTest;

[Singleton]
public class RuntimeTestAndLog : IRuntimeTestAndLog
{
    private readonly IRuntimeTestProcessor _testProcessor;
    private readonly ILogger _logger;

    public RuntimeTestAndLog(IRuntimeTestProcessor testProcessor, ILogger logger)
    {
        _testProcessor = testProcessor;
        _logger = logger;
    }

    public void Test()
    {
        var results = _testProcessor.Test<RuntimeTestAndLog>();
        var builder = new StringBuilder();

        builder.AppendLine(
            $"Runtime tests | {results.Count} total | {results.Count(x => x.Passed)} passed | {results.Count(x => !x.Passed)} failed");

        results.OrderBy(x => x.Passed).ThenBy(x => x.TestName).ToList().ForEach(x =>
        {
            builder.AppendLine(
                "------------------------------------------------------------------------------------------------------------------------");

            builder.AppendLine($"Test: {x.TestName} " + (x.Passed ? "Passed" : "Failed"));

            if (x.Exception != null)
            {
                builder.AppendLine("Exception:");
                builder.AppendLine(x.Exception.ToString());
            }
        });

        builder.AppendLine(
            "------------------------------------------------------------------------------------------------------------------------");

        _logger.LogInfo($"\n{builder}");
    }
}