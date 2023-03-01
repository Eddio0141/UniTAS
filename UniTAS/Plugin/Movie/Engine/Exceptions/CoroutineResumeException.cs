namespace UniTAS.Plugin.Movie.Engine.Exceptions;

public class CoroutineResumeException : MovieEngineException
{
    public CoroutineResumeException(string message) : base(message)
    {
    }
}