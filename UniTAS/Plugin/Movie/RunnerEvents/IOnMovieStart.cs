namespace UniTAS.Plugin.Movie.RunnerEvents;

public interface IOnMovieStart
{
    /// <summary>
    /// Invoked when the movie starts
    /// Invoked before the first frame of the TAS but after the parsing of the input file
    /// </summary>
    void OnMovieStart();
}