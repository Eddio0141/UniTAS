namespace UniTAS.Plugin.Movie.Exceptions;

public class MovieAlreadyRunningException : MovieRunnerException
{
    public MovieAlreadyRunningException() : base("Movie is already running")
    {
    }
}