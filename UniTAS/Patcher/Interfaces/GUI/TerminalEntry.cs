using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Interfaces.GUI;

/// <summary>
/// Defines a terminal entry.
/// </summary>
[RegisterAll]
public abstract class TerminalEntry
{
    /// <summary>
    /// An unique command used to identify this entry.
    /// </summary>
    public abstract string Command { get; }

    /// <summary>
    /// Description of this entry to be displayed in the help command.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Method that gets executed when the command is called.
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <param name="terminalWindow">The terminal window instance invoking this method</param>
    /// <returns>Return true to hijack the terminal window and prevent other entries from executing. To revert the hijacked state, check <see cref="ITerminalWindow.ReleaseTerminal"/></returns>
    public abstract bool Execute(string[] args, ITerminalWindow terminalWindow);

    /// <summary>
    /// Invoked when the terminal receives input while the terminal is hijacked.
    /// </summary>
    public virtual void OnInput(string input)
    {
    }
}