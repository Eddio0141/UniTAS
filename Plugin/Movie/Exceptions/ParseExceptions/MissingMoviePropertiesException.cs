namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class MissingMoviePropertiesException : MovieParseException
{
    public MissingMoviePropertiesException() : base("Missing properties")
    {
    }
}