namespace UniTASPlugin.Movie.Exceptions;

public class MissingMovieScriptException : MovieParseException
{
    public MissingMovieScriptException() : base("Missing script") { }
}