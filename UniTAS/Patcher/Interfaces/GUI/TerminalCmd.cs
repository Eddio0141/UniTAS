using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.GUI;

namespace UniTAS.Patcher.Interfaces.GUI;

/// <summary>
/// Defines a terminal command
/// </summary>
[RegisterAll]
public abstract class TerminalCmd
{
    /// <summary>
    /// Command name
    /// </summary>
    public abstract string Command { get; }

    /// <summary>
    /// Description of this command. It will be displayed in the help command
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Method that gets executed when the command is called
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <param name="terminalWindow">The terminal window instance invoking this method</param>
    /// <returns>Return true to hijack the terminal window and prevent other entries from executing. To revert the hijacked state, check <see cref="ITerminalWindow.ReleaseTerminal"/></returns>
    public abstract bool Execute(string[] args, ITerminalWindow terminalWindow);

    /// <summary>
    /// Executed when the terminal receives input while the terminal is hijacked
    /// </summary>
    /// <param name="input">Input string. If not split, this is the full complete input (with newlines if split). If split, it contains the split input</param>
    /// <param name="split">If the input is split or not</param>
    public virtual void OnInput(string input, bool split)
    {
    }
}
