using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;

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
    public abstract string Name { get; }

    /// <summary>
    /// Description of this command. It will be displayed in the help command
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Function to add to the terminal interpreter
    /// </summary>
    public abstract Delegate Callback { get; }

    /// <summary>
    /// Called once on setting up command
    /// </summary>
    public virtual void Setup()
    {
    }
}