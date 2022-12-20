namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class MissingMoviePropertiesException : MovieParseException
{
    public MissingMoviePropertiesException() : base("Missing properties")
    {
    }
}