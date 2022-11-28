namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class MissingResolutionFieldException : MovieParseException
{
    public MissingResolutionFieldException(string side) : base($"Missing resolution field {side}") { }
}