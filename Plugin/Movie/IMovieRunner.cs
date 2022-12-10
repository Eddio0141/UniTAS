namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    void RunFromInput(string input);

    void Update();
}