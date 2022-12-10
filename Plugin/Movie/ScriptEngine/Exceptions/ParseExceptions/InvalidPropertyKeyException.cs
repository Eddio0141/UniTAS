namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class InvalidPropertyKeyException : MovieParseException
{
    public InvalidPropertyKeyException(string key) : base($"Property key {key} doesn't exist")
    {
    }
}