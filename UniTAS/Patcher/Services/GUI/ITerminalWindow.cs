using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface ITerminalWindow
{
    /// <summary>
    /// Prints a line to the terminal window output.
    /// </summary>
    void TerminalPrintLine(string line);

    /// <summary>
    /// Invoke to release the hijacked terminal window.
    /// </summary>
    void ReleaseTerminal();

    /// <summary>
    /// All terminal commands.
    /// </summary>
    TerminalCmd[] TerminalCmds { get; }
}
