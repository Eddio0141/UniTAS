using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    bool MovieEnd { get; }

    VirtualEnvironment RunFromInput(string input, VirtualEnvironment env);

    VirtualEnvironment Update(VirtualEnvironment env);
}