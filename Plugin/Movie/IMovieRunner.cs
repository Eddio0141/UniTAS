namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    void AdvanceFrame();

    bool MovieEnd { get; }
}