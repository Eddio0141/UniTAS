namespace UniTAS.Plugin.Movie.Events;

public interface IOnMovieRunningStatusChange
{
    void OnMovieRunningStatusChange(bool running);
}