namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ParseExceptions;

public class DuplicateMethodsDefinedException : MovieParseException
{
    public DuplicateMethodsDefinedException() : base("Duplicate method names defined")
    {
    }
}