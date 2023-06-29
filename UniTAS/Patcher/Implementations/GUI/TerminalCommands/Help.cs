using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Help : TerminalEntry
{
    public override string Command => "help";

    public override string Description => "Prints all available commands";

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        var terminalEntries = terminalWindow.TerminalEntries;
        terminalWindow.TerminalPrintLine("Available commands:");

        foreach (var terminalEntry in terminalEntries)
        {
            terminalWindow.TerminalPrintLine($"{terminalEntry.Command} - {terminalEntry.Description}");
        }

        return false;
    }
}