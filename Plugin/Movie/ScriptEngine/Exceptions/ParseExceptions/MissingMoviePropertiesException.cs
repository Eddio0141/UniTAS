namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class MissingMoviePropertiesException : MovieParseException
{
    public MissingMoviePropertiesException() : base("Missing properties") { }
}