namespace UniTASPlugin.Movie.Exceptions;

public class MovieParseException : MovieException
{
    public MovieParseException(string message) : base($"Failed to parse movie: {message}") { }
}