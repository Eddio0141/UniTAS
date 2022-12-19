namespace UniTASPlugin.Movie.MovieRunner.Exceptions.ScriptEngineExceptions;

public class ValueTypeMismatchException : MovieScriptEngineException
{
    public ValueTypeMismatchException(string expectedType, string actualType) : base(
        $"Expected value type to be {expectedType} but got mismatched type {actualType}")
    {
    }
}