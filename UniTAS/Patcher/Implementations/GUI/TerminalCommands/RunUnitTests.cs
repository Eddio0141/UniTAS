using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Models.RuntimeTest;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.RuntimeTest;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class RunUnitTests : TerminalEntry
{
    public override string Command => "run_tests";
    public override string Description => "runs defined runtime unit tests with current game";

    private readonly IRuntimeTestProcessor _runtimeTestProcessor;

    private ITerminalWindow _terminalWindow;

    public RunUnitTests(IRuntimeTestProcessor runtimeTestProcessor)
    {
        _runtimeTestProcessor = runtimeTestProcessor;
    }

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        _runtimeTestProcessor.Test<RunUnitTests>();
        _runtimeTestProcessor.OnTestEnd += TestFinish;

        _terminalWindow = terminalWindow;

        // wait till tests are finished
        return true;
    }

    private void TestFinish(TestResults testResults)
    {
        _terminalWindow.TerminalPrintLine(testResults.ToString());
        _terminalWindow.ReleaseTerminal();
    }
}