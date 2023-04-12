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

        builder.Append($"Runtime tests | {results.Count} total | ");

        var passedCount = results.Count(x => x.Passed);
        var failedCount = results.Count(x => !x.Passed);
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

        builder.AppendLine();

        results.OrderBy(x => x.Passed).ThenBy(x => x.TestName).ToList().ForEach(x =>
        {
            builder.AppendLine(
                "------------------------------------------------------------------------------------------------------------------------");

            builder.AppendLine($"{x.TestName} " + (x.Passed ? "Passed" : "Failed"));

            if (x.Exception != null)
            {
                builder.AppendLine("\nException:");
                builder.AppendLine(x.Exception.ToString());
            }
        });

        builder.AppendLine(
            "------------------------------------------------------------------------------------------------------------------------");

        _logger.LogInfo($"\n{builder}");
    }
}