using System.Linq;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Help : TerminalEntry
{
    public override string Command => "help";

    public override string Description =>
        "Prints all available commands. Arg 0 (string) (optional): command to get help for";

    public override bool Execute(string[] args, ITerminalWindow terminalWindow)
    {
        var command = args.Length > 0 ? args[0] : null;

        if (command != null)
        {
            // display help for a command
            var terminalEntry = terminalWindow.TerminalEntries.FirstOrDefault(x => x.Command == command);

            terminalWindow.TerminalPrintLine(terminalEntry == null
                ? $"Command {command} not found"
                : $"{terminalEntry.Command} - {terminalEntry.Description}");

            return false;
        }

        var terminalEntries = terminalWindow.TerminalEntries;
        terminalWindow.TerminalPrintLine("Available commands:");

        foreach (var terminalEntry in terminalEntries)
        {
            terminalWindow.TerminalPrintLine($"{terminalEntry.Command} - {terminalEntry.Description}");
        }

        return false;
    }
}