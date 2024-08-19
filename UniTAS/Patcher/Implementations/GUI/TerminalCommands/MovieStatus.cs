using System;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.GUI;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.GUI.TerminalCommands;

public class MovieStatus(IMovieRunnerEvents movieRunnerEvents) : TerminalCmd
{
    public override string Name => "movie_status";
    public override string Description => "gets an object that contains movie status";
    public override Delegate Callback => Execute;

    private Status Execute()
    {
        return new(movieRunnerEvents);
    }

    private class Status : IDisposable
    {
        private readonly IMovieRunnerEvents _movieRunnerEvents;

        [MoonSharpHidden]
        public Status(IMovieRunnerEvents movieRunnerEvents)
        {
            _movieRunnerEvents = movieRunnerEvents;
            movieRunnerEvents.OnMovieSetup += SetupStart;
            movieRunnerEvents.OnMovieEnd += MovieEnd;
        }

        private void SetupStart()
        {
            BasicallyRunning = true;
        }

        private void MovieEnd()
        {
            BasicallyRunning = false;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public bool BasicallyRunning { get; private set; }

        public void Dispose()
        {
            _movieRunnerEvents.OnMovieSetup -= SetupStart;
            _movieRunnerEvents.OnMovieEnd -= MovieEnd;
        }
    }
}