namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }
    public ulong FrameCount { get; }

    void RunFromInput(string input);
    void Update();
}