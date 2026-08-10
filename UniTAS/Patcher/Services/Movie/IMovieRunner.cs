using UniTAS.Patcher.Models.Movie;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Services.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }
    bool SetupOrMovieRunning { get; }
    IMovieLogger MovieLogger { get; }
    void RunFromPath(string path);
    UpdateType UpdateType { set; }
}
