namespace UniTAS.Plugin.Movie.Exceptions.ScriptEngineExceptions;

public class MovieAlreadyRunningException : MovieScriptEngineException
{
    public MovieAlreadyRunningException() : base("Movie is already running")
    {
    }
}