namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ScriptEngineExceptions;

public class MovieAlreadyRunningException : MovieScriptEngineException
{
    public MovieAlreadyRunningException() : base("Movie is already running")
    {
    }
}