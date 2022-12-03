namespace UniTASPlugin.Movie.ScriptEngine.EngineMethods.Exceptions;

public class RegisterExternalMethodException : ExternalMethodRuntimeException
{
    public RegisterExternalMethodException(string message) : base(message)
    {
    }
}