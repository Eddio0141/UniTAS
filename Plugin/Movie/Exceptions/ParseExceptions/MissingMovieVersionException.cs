namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class MissingMovieVersionException : MovieParseException
{
    public MissingMovieVersionException() : base("Missing movie version key") { }
}