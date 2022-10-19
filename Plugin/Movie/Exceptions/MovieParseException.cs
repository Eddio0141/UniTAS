using System;

namespace UniTASPlugin.Movie.Exceptions;

public class MovieParseException : Exception
{
    public MovieParseException(string message) : base($"Failed to parse movie: {message}") { }
}