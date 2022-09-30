using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTASPlugin.GameOverlay.GameConsole;

public static class AllCommands
{
    public static List<Command> Commands { get; } = new()
    {
        new Command("testing", "", args => {
            Console.Print("testing");
            Console.Print($"all params: {string.Join(", ", args.Select(x => x.ToString()).ToArray())}");
            Console.Print($"param count: {args.Length}");
        }),
        new Command("clear", "Clears the terminal.", args => {
            if (!ValidateArgCount(args, 0))
                return;
            Console.Clear();
        }, usage: "clear();"),
        new Command("echo", "Prints the given text to the console.", args => {
            if (!ValidateArgCount(args, 1))
                return;
            Console.Print(args[0].ToString());
        }, usage: "echo(<object>);"),
        new Command("soft_restart", "Simulates a game restart, no args will use the in-game time as the seed.", args =>
        {
            if (!ValidateArgCount(args, 0, 1))
                return;
            if (args.Length == 0)
                GameRestart.SoftRestart(DateTime.Now);
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
                GameRestart.SoftRestart(dateTime);
            }
        }, usage: "soft_restart(); / soft_restart(\"DateTime string\");"),
    };

    static bool ValidateArgCount(Parameter[] args, int count)
    {
        if (args.Length != count)
        {
            Console.Print($"Invalid argument count, expected {count} args, got {args.Length}");
            return false;
        }
        return true;
    }

    static bool ValidateArgCount(Parameter[] args, int minInclusive, int maxInclusive)
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
