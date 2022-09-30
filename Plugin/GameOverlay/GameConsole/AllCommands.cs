using System;
using System.Collections.Generic;

namespace UniTASPlugin.GameOverlay.GameConsole;

public static class AllCommands
{
    public static List<Command> commands { get; } = new()
    {
        new Command("echo", "Prints the given text to the console.", (parameters) => {
            if (!ValidateArgCount(parameters, 1))
                return;
            Console.Print(parameters[0].ToString());
        }, usage: "echo(<object>)"),
        new Command("soft_restart", "Simulates a game restart, no args will use the in-game time as the seed.", args =>
        {
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
        }, usage: "soft_restart() / soft_restart(\"DateTime string\")"),
    };

    static bool ValidateArgCount(Parameter[] parameters, int count)
    {
        if (parameters.Length != count)
        {
            Console.Print($"Invalid argument count, expected {count}, got {parameters.Length}");
            return false;
        }
        return true;
    }
}
