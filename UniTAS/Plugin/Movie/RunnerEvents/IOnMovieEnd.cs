namespace UniTAS.Plugin.Movie.RunnerEvents;

public interface IOnMovieEnd
{
    /// <summary>
    /// Invoked when the movie ends
    /// Only invoked if the movie is finished or stopped in any way while running
    /// </summary>
    void OnMovieEnd();
}