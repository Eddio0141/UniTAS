namespace UniTASPlugin.Movie.Exceptions.ScriptEngineExceptions;

public class MovieMethodAlreadyDefinedException : MovieScriptEngineException
{
    public MovieMethodAlreadyDefinedException() : base("Method is already defined")
    {
    }
}