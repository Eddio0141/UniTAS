namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

public class ConflictingPropertyKeyException : MovieParseException
{
    public ConflictingPropertyKeyException(string key, string conflictKey) : base(
        $"Key {key} conflicts with {conflictKey}")
    {
    }
}