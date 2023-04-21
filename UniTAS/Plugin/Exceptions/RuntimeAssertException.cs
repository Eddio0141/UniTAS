namespace UniTAS.Plugin.Exceptions;

public class RuntimeAssertException : System.Exception
{
    public RuntimeAssertException(string message) : base(message)
    {
    }
}