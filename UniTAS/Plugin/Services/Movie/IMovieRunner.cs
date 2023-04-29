using System;
using UniTAS.Plugin.Models.Movie;

namespace UniTAS.Plugin.Services.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    void RunFromInput(string input);

    UpdateType UpdateType { set; }

    event Action OnMovieStart;
    event Action OnMovieEnd;
}