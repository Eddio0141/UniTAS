using UniTASPlugin.GameEnvironment;

namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    void AdvanceFrame();

    bool MovieEnd { get; }

    VirtualEnvironment RunFromInput(string input, VirtualEnvironment env);

    VirtualEnvironment Update(VirtualEnvironment env);
}