namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ScriptEngineExceptions;

public class UsingUndefinedVariableException : MovieScriptEngineException
{
    public UsingUndefinedVariableException(string varName) : base($"Use of undefined variable {varName}")
    {
    }
}