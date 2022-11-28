using UniTASPlugin.GameEnvironment.Interfaces;

namespace UniTASPlugin.Movie;

public interface IMovieRunner
{
    void AdvanceFrame();

    bool MovieEnd { get; }

    void RunFromPath<TEnv>(string path, ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty;

    void Update<TEnv>(ref TEnv env)
        where TEnv :
        IRunVirtualEnvironmentProperty,
        IInputStateProperty;
}