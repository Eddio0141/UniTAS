using System;

namespace UniTASPlugin.Movie.Exceptions;

public class MovieException : Exception
{
    public MovieException(string message) : base(message) { }
}