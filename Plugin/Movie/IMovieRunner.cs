using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    void RunFromInput(string input, VirtualEnvironment env);

    void Update(VirtualEnvironment env);
}