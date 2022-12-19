namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ScriptEngineExceptions;

public class MovieMethodAlreadyDefinedException : MovieScriptEngineException
{
    public MovieMethodAlreadyDefinedException() : base("Method is already defined")
    {
    }
}