namespace UniTAS.Patcher.Exceptions;

public class RuntimeAssertException : System.Exception
{
    public RuntimeAssertException(string message) : base(message)
    {
    }
}