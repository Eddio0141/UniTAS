using System;

namespace UniTASPlugin.GameOverlay.GameConsole;

public class Command
{
    public readonly string Name;
    public readonly string Description;
    public readonly string Usage;
    public readonly string[] Aliases;
    public readonly Action<Parameter[]> Action;

    public Command(string name, string description, string[] aliases, string usage, Action<Parameter[]> action)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Aliases = aliases ?? new string[0];
        Usage = usage ?? throw new ArgumentNullException(nameof(usage));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public Command(string name, string description, Action<Parameter[]> action, string usage = "", string[] aliases = null)
        : this(name, description, aliases, usage, action) { }

    public void Invoke(Parameter[] parameters)
    {
        Action.Invoke(parameters);
    }
}
