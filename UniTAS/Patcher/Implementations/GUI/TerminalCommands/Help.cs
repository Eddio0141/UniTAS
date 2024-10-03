using System;
using System.Linq;
using BepInEx;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class Help : TerminalCmd
{
    public override string Name => "help";

    public override string Description =>
        "Prints all available commands. Arg 0 (string) (optional): command to get help for";

    public override Delegate Callback => Execute;

    private static readonly TerminalCmd[] Commands = ContainerStarter.Kernel.GetAllInstances<TerminalCmd>().ToArray();

    private void Execute(Script script, string command)
    {
        var print = script.Options.DebugPrint;

        if (!command.IsNullOrWhiteSpace())
        {
            // display help for a command
            var terminalEntry = Commands.FirstOrDefault(x => x.Name == command);

            print(terminalEntry == null
                ? $"Command {command} not found"
                : $"{terminalEntry.Name} - {terminalEntry.Description}");
        }

        print("Available commands:");

        foreach (var terminalEntry in Commands)
        {
            print($"{terminalEntry.Name} - {terminalEntry.Description}");
        }
    }
}