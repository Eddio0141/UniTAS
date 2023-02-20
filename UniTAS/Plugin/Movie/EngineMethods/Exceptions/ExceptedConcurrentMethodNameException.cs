namespace UniTAS.Plugin.Movie.EngineMethods.Exceptions;

public class ExceptedConcurrentMethodNameException : RegisterExternalMethodException
{
    public ExceptedConcurrentMethodNameException() : base(
        "Wrong method name argument, need a string of a method name for concurrent running")
    {
    }
}