using System;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.RuntimeTest;
using UniTAS.Patcher.Services.RuntimeTest;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class RunUnitTests(IRuntimeTestProcessor runtimeTestProcessor) : TerminalCmd
{
    public override string Name => "test";

    public override string Description =>
        "Runs defined runtime unit tests with current game. Arg 0 (uint) (optional): how many times to run the tests";


    private uint _pendingTimes;

    public override Delegate Callback => Execute;

    private Action<string> _print;

    private void Execute(Script script, uint repeatTimes)
    {
        _print = script.Options.DebugPrint;
        _pendingTimes = Math.Max(1u, _pendingTimes);

        runtimeTestProcessor.OnTestsFinish += TestsFinish;
        runtimeTestProcessor.OnTestRun += TestRun;
        runtimeTestProcessor.OnTestEnd += TestEnd;
        runtimeTestProcessor.Test<RunUnitTests>();
    }

    private void TestsFinish(TestResults testResults)
    {
        _pendingTimes--;
        _print(testResults.ToString());

        if (_pendingTimes > 0)
        {
            _print($"re-running the tests {_pendingTimes} more times");
            runtimeTestProcessor.Test<RunUnitTests>();
            return;
        }

        runtimeTestProcessor.OnTestsFinish -= TestsFinish;
        runtimeTestProcessor.OnTestRun -= TestRun;
        runtimeTestProcessor.OnTestEnd -= TestEnd;
    }

    private void TestEnd(TestResult result)
    {
        _print($"Test finished : {result.TestName}");
        _print(result.Passed ? "passed" : result.Skipped ? "skipped" : "failed");
    }

    private void TestRun(string name)
    {
        _print($"Running test {name}");
    }
}