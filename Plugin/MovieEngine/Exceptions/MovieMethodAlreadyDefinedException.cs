namespace UniTASPlugin.MovieEngine.Exceptions;

public class MovieMethodAlreadyDefinedException : MovieEngineException
{
    public MovieMethodAlreadyDefinedException() : base("Method is already defined") { }
}