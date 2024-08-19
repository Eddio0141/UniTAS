using System;

namespace UniTAS.Patcher.Services.Movie;

public interface IMovieRunnerEvents
{
    event Action OnMovieStart;
    event Action OnMovieEnd;
    event Action OnMovieSetup;
}