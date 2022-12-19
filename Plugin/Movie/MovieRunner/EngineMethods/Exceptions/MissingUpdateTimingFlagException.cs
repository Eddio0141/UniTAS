namespace UniTASPlugin.Movie.MovieRunner.EngineMethods.Exceptions;

public class MissingUpdateTimingFlagException : RegisterExternalMethodException
{
    public MissingUpdateTimingFlagException() : base("Missing bool flag for running on update prefix")
    {
    }
}