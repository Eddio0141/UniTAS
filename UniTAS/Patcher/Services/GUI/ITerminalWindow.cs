using UniTAS.Patcher.Interfaces.GUI;

namespace UniTAS.Patcher.Services.GUI;

public interface ITerminalWindow
{
    /// <summary>
    /// All terminal commands.
    /// </summary>
    TerminalCmd[] TerminalCmds { get; }
}
