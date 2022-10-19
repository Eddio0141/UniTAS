namespace UniTASPlugin.Movie.ParseExceptions;

public class MissingMovieScriptException : MovieParseException
{
    public MissingMovieScriptException() : base("Missing script") { }
}