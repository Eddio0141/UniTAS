using System;
using System.Collections.Generic;
using System.Linq;

namespace UniTASPlugin.GameOverlay.GameConsole;

public static class Executor
{
    static Queue<KeyValuePair<Command, Parameter[]>> commandQueue = new();

    static void ExecuteCommands()
    {
        while (commandQueue.Count > 0)
        {
            var commandParam = commandQueue.Dequeue();
            var command = commandParam.Key;
            var args = commandParam.Value;
            command.Invoke(args);
        }
    }

    static void CommandSyntaxError(string err, string commandName)
    {
        Console.Print($"Command syntax error, {err}, command: {commandName}");
    }

    static void ArgumentSyntaxError(string err, string commandName, int argIndex)
    {
        Console.Print($"Argument syntax error, {err}, command: {commandName}, arg index: {argIndex}");
    }

    static void UnreachableError(string err, string commandName, int argIndex)
    {
        Console.Print($"Unreachable error, {err}, command: {commandName}, arg index: {argIndex}");
    }

    public static void Process(string input)
    {
        input = input.Trim();

        var commandName = true;
        var findingOpenBracket = true;
        var args = true;
        var argStart = true;
        var argsString = false;
        var argsList = false;
        var argsGettingValue = false;

        var commandNameBuilder = "";

        Command currentCommand = null;
        List<Parameter> currentArgs = new();
        var argBuilder = "";

        for (int i = 0; i < input.Length; i++)
        {
            var ch = input[i];

            if (commandName)
            {
                if (ch == ' ' || ch == '(')
                {
                    commandName = false;

                    // find command
                    var commandIndex = AllCommands.commands.FindIndex(c => c.Name == commandNameBuilder || c.Aliases.Contains(commandNameBuilder));
                    if (commandIndex < 0)
                    {
                        Console.Print($"Command {commandNameBuilder} not found");
                        return;
                    }
                    // check if index is at the end of the list
                    if (i + 1 == input.Length)
                    {
                        Console.Print($"Missing arguments for command {commandNameBuilder}");
                        return;
                    }
                    currentCommand = AllCommands.commands[commandIndex];
                }
                else
                {
                    commandNameBuilder += ch;
                    continue;
                }
            }
            if (findingOpenBracket)
            {
                if (ch == '(')
                {
                    findingOpenBracket = false;
                    continue;
                }
                else if (ch != ' ')
                {
                    Console.Print("Invalid command syntax, command name cannot contain spaces");
                    return;
                }
                // check if index is at the end of the list
                if (i + 1 == input.Length)
                {
                    if (ch == '(')
                        Console.Print($"Invalid command syntax, missing closing bracket for command {commandNameBuilder}");
                    else
                        Console.Print($"Invalid command syntax, missing arguments for command {commandNameBuilder}");
                    return;
                }
            }
            if (args)
            {
                if (argStart)
                {
                    if (ch == ')')
                    {
                        if (argsList)
                        {
                            ArgumentSyntaxError("the list closing bracket is missing", commandNameBuilder, currentArgs.Count);
                            return;
                        }
                        args = false;
                    }
                    else if (ch != ' ')
                    {
                        if (ch == '"')
                            argsString = true;
                        else if (ch == '[')
                        {
                            if (argsList)
                            {
                                Console.Print($"Invalid argument, you cannot use recursive lists, command: {commandNameBuilder}, arg index {currentArgs.Count}");
                                return;
                            }
                            // repeat normal process but we are now in a list
                            argsList = true;
                            continue;
                        }

                        argStart = false;
                        argsGettingValue = true;
                    }
                    if (i + 1 == input.Length)
                    {
                        if (ch == ')')
                            Console.Print($"Invalid command syntax, missing terminator for command {commandNameBuilder}");
                        else if (ch == '"')
                            Console.Print($"Invalid command syntax, missing closing string quote for command {commandNameBuilder}, arg index {currentArgs.Count}");
                        else if (ch == '[')
                            Console.Print($"Invalid command syntax, missing closing list quote for command {commandNameBuilder}, arg index {currentArgs.Count}");
                        else
                            Console.Print($"Invalid command syntax, missing closing bracket for command {commandNameBuilder}");
                        return;
                    }
                    continue;
                }
                if (argsGettingValue)
                {
                    // check string termination
                    if (argsString && ch == '"' && input[i - 1] != '\\')
                    {
                        if (!Parameter.FromString(argBuilder, out var arg) || arg.ParamType != ParameterType.String)
                        {
                            Console.Print($"Unreachable error, \"{argBuilder}\" should be parsed as a string for command {commandNameBuilder}, arg index {currentArgs.Count}");
                            return;
                        }
                        argsString = false;
                        argsGettingValue = false;
                        currentArgs.Add(arg);
                        continue;
                    }
                    // check list termination
                    if (!argsString && argsList && ch == ']')
                    {
                        if (!Parameter.FromString(argBuilder, out var arg) || arg.ParamType != ParameterType.List)
                        {
                            ArgumentSyntaxError($"argument {argBuilder} failed to parse as a list, make sure the values in the list are all the same", commandNameBuilder, currentArgs.Count);
                            return;
                        }
                        argsList = false;
                        argsGettingValue = false;
                        currentArgs.Add(arg);
                        continue;
                    }
                    // check normal value termination
                    if (!argsString && !argsList && ch == ',')
                    {
                        if (!Parameter.FromString(argBuilder, out var arg))
                        {
                            ArgumentSyntaxError($"argument {argBuilder} failed to parse", commandNameBuilder, currentArgs.Count);
                            return;
                        }
                        argsGettingValue = false;
                        currentArgs.Add(arg);
                    }
                    // add to builder unless normal value termination
                    else
                        argBuilder += ch;

                    // check list separation
                    if (!argsString && argsList && ch == ',')
                    {
                        argsGettingValue = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                // check for arg end
                if (ch == ',' || ch == ')')
                    argStart = true;
                else if (ch != ' ')
                {
                    Console.Print($"Invalid arguments syntax, expected a separator or an argument closing bracket for command {commandNameBuilder}");
                    return;
                }
                continue;
            }

            if (ch != ' ' && ch != ';')
            {
                Console.Print($"Invalid command syntax, missing terminator for command {commandNameBuilder}");
                return;
            }

            if (ch == ';')
            {
                commandQueue.Enqueue(new KeyValuePair<Command, Parameter[]>(currentCommand, currentArgs.ToArray()));

                currentCommand = null;
                currentArgs.Clear();
            }
        }


        ExecuteCommands();
    }
}
