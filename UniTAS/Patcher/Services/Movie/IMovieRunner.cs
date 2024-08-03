using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Services.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }
    IMovieLogger MovieLogger { get; }
    void RunFromInput(string input);
    UpdateType UpdateType { set; }
}