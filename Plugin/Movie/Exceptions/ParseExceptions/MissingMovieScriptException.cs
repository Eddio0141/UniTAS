namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class MissingMovieScriptException : MovieParseException
{
    public MissingMovieScriptException() : base("Missing script") { }
}