namespace UniTASPlugin.Movie.Exceptions.ParseExceptions;

public class InvalidPropertyKeyException : MovieParseException
{
    public InvalidPropertyKeyException() : base("Property key doesn't exist") { }
}