namespace UniTASPlugin.Movie.ScriptEngine.Exceptions.ScriptEngineExceptions;

public class UsingUndefinedVariableException : MovieScriptEngineException
{
    public UsingUndefinedVariableException(string varName) : base($"Use of undefined variable {varName}")
    {
    }
}