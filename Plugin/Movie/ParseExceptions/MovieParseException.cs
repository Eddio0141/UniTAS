using System;

namespace UniTASPlugin.Movie.ParseExceptions;

public class MovieParseException : Exception
{
    public MovieParseException(string message) : base($"Failed to parse movie: {message}") { }
}