namespace UniTASPlugin.Movie.Exceptions;

public class MissingMoviePropertiesException : MovieParseException
{
    public MissingMoviePropertiesException() : base("Missing properties") { }
}