using System;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.RuntimeTest;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.RuntimeTest;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class RunUnitTests : TerminalEntry
{
    public override string Command => "test";

    public override string Description =>
        "Runs defined runtime unit tests with current game. Arg 0 (uint) (optional): how many times to run the tests";

    private readonly IRuntimeTestProcessor _runtimeTestProcessor;

    private ITerminalWindow _terminalWindow;

    private uint _pendingTimes;

    public RunUnitTests(IRuntimeTestProcessor runtimeTestProcessor)
    {
        _runtimeTestProcessor = runtimeTestProcessor;
    }

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        var timesRaw = args.Length > 0 ? args[0] : null;
        _pendingTimes = 1u;
        if (timesRaw != null)
        {
            if (!uint.TryParse(timesRaw, out _pendingTimes))
            {
                terminalWindow.TerminalPrintLine("Argument 0 must be an integer");
                return false;
            }

            _pendingTimes = Math.Max(1u, _pendingTimes);
        }

        _terminalWindow = terminalWindow;

        _runtimeTestProcessor.OnTestsFinish += TestsFinish;
        _runtimeTestProcessor.OnTestRun += TestRun;
        _runtimeTestProcessor.OnTestEnd += TestEnd;
        _runtimeTestProcessor.Test<RunUnitTests>();

        // wait till tests are finished
        return true;
    }

    private void TestsFinish(TestResults testResults)
    {
        _pendingTimes--;
        _terminalWindow.TerminalPrintLine(testResults.ToString());

        if (_pendingTimes > 0)
        {
            _terminalWindow.TerminalPrintLine($"re-running the tests {_pendingTimes} more times");
            _runtimeTestProcessor.Test<RunUnitTests>();
            return;
        }

        _runtimeTestProcessor.OnTestsFinish -= TestsFinish;
        _runtimeTestProcessor.OnTestRun -= TestRun;
        _runtimeTestProcessor.OnTestEnd -= TestEnd;

        _terminalWindow.ReleaseTerminal();
    }

    private void TestEnd(TestResult result)
    {
        _terminalWindow.TerminalPrintLine($"Test finished : {result.TestName}");
        _terminalWindow.TerminalPrintLine(result.Passed ? "passed" : result.Skipped ? "skipped" : "failed");
    }

    private void TestRun(string name)
    {
        _terminalWindow.TerminalPrintLine($"Running test {name}");
    }
}