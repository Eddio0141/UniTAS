namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class MissingMovieScriptException : MovieParseException
{
    public MissingMovieScriptException() : base("Missing script")
    {
    }
}