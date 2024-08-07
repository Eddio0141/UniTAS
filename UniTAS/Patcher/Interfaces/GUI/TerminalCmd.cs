using System;
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
    /// Terminal window the command is used in (if instance is used in one)
    /// </summary>
    public ITerminalWindow TerminalWindow { get; set; }
    
    /// <summary>
    /// Command name
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Description of this command. It will be displayed in the help command
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Function to add to the terminal interpreter
    /// </summary>
    public abstract Delegate Callback { get; }
}
