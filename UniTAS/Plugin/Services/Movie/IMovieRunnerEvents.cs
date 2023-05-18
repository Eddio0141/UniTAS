using System;

namespace UniTAS.Plugin.Services.Movie;

public interface IMovieRunnerEvents
{
    event Action OnMovieStart;
    event Action OnMovieEnd;
}