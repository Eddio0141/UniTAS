namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ScriptEngineExceptions;

public class MovieAlreadyRunningException : MovieScriptEngineException
{
    public MovieAlreadyRunningException() : base("Movie is already running")
    {
    }
}