namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class DuplicatePropertyKeyException : MovieParseException
{
    public DuplicatePropertyKeyException() : base("Property key was already processed") { }
    public DuplicatePropertyKeyException(string altKeyName) : base($"Property key was already processed through alternative key: {altKeyName}") { }
}