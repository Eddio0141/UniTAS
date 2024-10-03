using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace UniTAS.Patcher.Models.RuntimeTest;

public class TestResults
{
    public ReadOnlyCollection<TestResult> Results { get; }
    public int PassedCount { get; }
    public int FailedCount { get; }
    public int SkippedCount { get; }

    public TestResults(IEnumerable<TestResult> results)
    {
        Results = results.OrderBy(x => x.Passed).ThenBy(x => x.TestName).ToList().AsReadOnly();

        PassedCount = Results.Count(x => x.Passed);
        FailedCount = Results.Count(x => !x.Passed && !x.Skipped);
        SkippedCount = Results.Count(x => x.Skipped);
    }

    private string _stringResult;

    public override string ToString()
    {
        if (_stringResult != null)
        {
            return _stringResult;
        }

        var builder = new StringBuilder();

        builder.Append($"Runtime tests | {Results.Count} total | ");

        if (PassedCount > 0)
        {
            builder.Append($"{PassedCount} passed");
        }

        if (PassedCount > 0 && FailedCount > 0)
        {
            builder.Append(" | ");
        }

        if (FailedCount > 0)
        {
            builder.Append($"{FailedCount} failed");
        }

        if ((PassedCount > 0 || FailedCount > 0) && SkippedCount > 0)
        {
            builder.Append(" | ");
        }

        if (SkippedCount > 0)
        {
            builder.Append($"{SkippedCount} skipped");
        }

        builder.AppendLine();

        Results.ToList().ForEach(x =>
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

        _stringResult = builder.ToString();
        return _stringResult;
    }
}