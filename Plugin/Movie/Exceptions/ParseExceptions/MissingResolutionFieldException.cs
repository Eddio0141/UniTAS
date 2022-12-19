namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class MissingResolutionFieldException : MovieParseException
{
    public MissingResolutionFieldException(string side) : base($"Missing resolution field {side}")
    {
    }
}