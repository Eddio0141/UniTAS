namespace UniTAS.Plugin.Exceptions.Movie.Runner;

public class MovieAlreadyRunningException : MovieRunnerException
{
    public MovieAlreadyRunningException() : base("Movie is already running")
    {
    }
}