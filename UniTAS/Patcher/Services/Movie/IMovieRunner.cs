using UniTAS.Patcher.Models.Movie;

namespace UniTAS.Patcher.Services.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    void RunFromInput(string input);

    UpdateType UpdateType { set; }
}