namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class UnknownMovieStartOptionException : MovieParseException
{
    public UnknownMovieStartOptionException() : base("Unknown movie start option from the set flags")
    {
    }
}