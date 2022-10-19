namespace UniTASPlugin.Movie.ParseExceptions;

public class MissingMoviePropertiesException : MovieParseException
{
    public MissingMoviePropertiesException() : base("Missing properties") { }
}