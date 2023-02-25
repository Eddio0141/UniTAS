namespace UniTAS.Plugin.Movie.Engine;

public interface IMovieEngine
{
    void Update();
    bool Finished { get; }
}