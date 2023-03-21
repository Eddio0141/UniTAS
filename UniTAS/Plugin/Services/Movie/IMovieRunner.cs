﻿using System;

namespace UniTAS.Plugin.Services.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    void RunFromInput(string input);
    
    event Action OnMovieStart;
    event Action OnMovieEnd;
}