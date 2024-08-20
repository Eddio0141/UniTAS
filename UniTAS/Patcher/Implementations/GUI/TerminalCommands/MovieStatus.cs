using System;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class MovieStatus(IMovieRunner movieRunner) : TerminalCmd
{
    public override string Name => "movie_status";
    public override string Description => "gets an object that contains movie status";
    public override Delegate Callback => Execute;

    private Status Execute()
    {
        return new(movieRunner);
    }

    [method: MoonSharpHidden]
    private class Status(IMovieRunner movieRunner)
    {
        [UsedImplicitly] public bool BasicallyRunning => movieRunner.SetupOrMovieRunning;
    }
}