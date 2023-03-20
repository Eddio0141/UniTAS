namespace UniTAS.Plugin.Exceptions.Movie.Engine;

public class CoroutineResumeException : MovieEngineException
{
    public CoroutineResumeException(string message) : base(message)
    {
    }
}