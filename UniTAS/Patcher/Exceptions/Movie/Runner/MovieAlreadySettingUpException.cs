namespace UniTAS.Patcher.Exceptions.Movie.Runner;

public class MovieAlreadySettingUpException : MovieRunnerException
{
    public MovieAlreadySettingUpException() : base("Movie is already running")
    {
    }
}