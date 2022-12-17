using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniTASPlugin.GameOverlay.GameConsole;

public static class AllCommands
{
    public static List<Command> Commands { get; } = new()
    {
        new("test", "", args =>
        {
            Console.Print("testing");
            Console.Print($"all args: {string.Join(", ", args.Select(x => x.ToString()).ToArray())}");
            Console.Print($"arg types: {string.Join(", ", args.Select(x => x.ParamType.ToString()).ToArray())}");
            Console.Print($"arg count: {args.Length}");
        }),
        new("commands", "Lists all commands.", args =>
        {
            if (!ValidateArgCount(args, 0))
                return;
            Console.Print("all commands:");
            foreach (var cmd in Commands)
            {
                Console.Print(cmd.Name);
            }
        }),
        new("clear", "Clears the terminal.", args =>
        {
            if (!ValidateArgCount(args, 0))
                return;
            Console.Clear();
        }, usage: "clear();"),
        new("help", "Help for the terminal or a specific command. Run commands(); for list of commands.", args =>
        {
            if (!ValidateArgCount(args, 0, 1))
                return;
            Command helpCmd;
            if (args.Length == 0)
            {
                helpCmd = Commands.Find(c => c.Name == "help");
            }
            else if (!args[0].GetString(out var findName))
            {
                Console.Print("Argument needs to be the command name string");
                return;
            }
            else
            {
                var helpCmdIndex = Commands.FindIndex(c => c.Name == findName || c.Aliases.Contains(findName));
                if (helpCmdIndex < 0)
                {
                    Console.Print("Command not found");
                    return;
                }

                helpCmd = Commands[helpCmdIndex];
            }

            Console.Print($"Command {helpCmd.Name}");
            if (helpCmd.Aliases.Length > 0)
                Console.Print($"aliases: {string.Join(" ", helpCmd.Aliases)}");
            if (helpCmd.Description != "")
                Console.Print($"description: {helpCmd.Description}");
            if (helpCmd.Usage != "")
                Console.Print($"usage: {helpCmd.Usage}");
        }, "Help(); / Help(\"echo\");"),
        new("quit", "Quits the game.", args =>
        {
            if (!ValidateArgCount(args, 0))
                return;
            Application.Quit();
        }, usage: "quit();"),
        new("echo", "Prints the given text to the console.", args =>
        {
            if (!ValidateArgCount(args, 1))
                return;
            Console.Print(args[0].ToString());
        }, usage: "echo(<object>);"),
        new("soft_restart", "Simulates a game restart, no args will use the in-game time as the seed.", args =>
        {
            if (!ValidateArgCount(args, 0, 1))
                return;
            var restart = Plugin.Kernel.GetInstance<IGameRestart>();
            if (args.Length == 0)
            {
                Console.Print($"Restarting with time {DateTime.Now}");
                restart.SoftRestart(DateTime.Now);
            }
            else
            {
                if (!args[0].GetString(out var dateTimeString))
                {
                    Console.Print("Argument 0 isn't a string");
                    return;
                }

                if (!DateTime.TryParse(dateTimeString, out var dateTime))
                {
                    Console.Print("Invalid DateTime value");
                    return;
                }

                Console.Print($"Restarting with time {dateTime}");
                restart.SoftRestart(DateTime.Now);
            }
        }, usage: "soft_restart(); / soft_restart(\"DateTime string\");")
    };

    private static bool ValidateArgCount(Parameter[] args, int count)
    {
        if (args.Length != count)
        {
            Console.Print($"Invalid argument count, expected {count} args, got {args.Length}");
            return false;
        }

        return true;
    }

    private static bool ValidateArgCount(Parameter[] args, int minInclusive, int maxInclusive)
    {
        if (args.Length < minInclusive)
        {
            Console.Print($"Invalid argument count, expected at least {minInclusive} args, got {args.Length}");
            return false;
        }

        if (args.Length > maxInclusive)
        {
            Console.Print($"Invalid argument count, expected at most {maxInclusive} args, got {args.Length}");
            return false;
        }

        return true;
    }
}