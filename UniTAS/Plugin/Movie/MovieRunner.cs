using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.Exceptions.ScriptEngineExceptions;
using UniTAS.Plugin.Movie.Parsers.EngineParser;

namespace UniTAS.Plugin.Movie;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MovieRunner : IMovieRunner, IOnPreUpdates
{
    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly IGameRestart _gameRestart;

    private readonly ISyncFixedUpdate _syncFixedUpdate;

    public bool MovieEnd { get; private set; } = true;
    private bool _cleanUp;
    private bool _setup;

    private readonly IMovieEngineParser _parser;
    private IMovieEngine _engine;

    public MovieRunner(VirtualEnvironment vEnv, IGameRestart gameRestart, ISyncFixedUpdate syncFixedUpdate,
        IMovieEngineParser parser)
    {
        _virtualEnvironment = vEnv;
        _gameRestart = gameRestart;
        _syncFixedUpdate = syncFixedUpdate;
        _parser = parser;
    }

    public void RunFromInput(string input)
    {
        if (!MovieEnd || _setup) throw new MovieAlreadyRunningException();

        _setup = true;

        _engine = _parser.Parse(input);

        // set env from properties
        _virtualEnvironment.RunVirtualEnvironment = true;

        // if (startupProperties != null)
        // {
        //     Trace.Write($"Using startup property: {startupProperties}");
        //     _virtualEnvironment.FrameTime = startupProperties.FrameTime;
        //     _gameRestart.SoftRestart(startupProperties.StartTime);
        // }

        // TODO other stuff like save state load, hide cursor, etc

        _syncFixedUpdate.OnSync(() =>
        {
            if (_gameRestart.PendingRestart)
            {
                _syncFixedUpdate.OnSync(() => { MovieEnd = false; }, 1, 1);
            }
            else
            {
                MovieEnd = false;
            }
        }, 1);
    }

    public void PreUpdate()
    {
        if (_cleanUp)
        {
            _virtualEnvironment.RunVirtualEnvironment = false;
            _cleanUp = false;
            return;
        }

        if (MovieEnd) return;

        // ConcurrentRunnersPreUpdate();
        _engine.Update();
        // ConcurrentRunnersPostUpdate();

        if (_engine.Finished)
        {
            AtMovieEnd();
        }
    }

    private void AtMovieEnd()
    {
        _virtualEnvironment.FrameTime = 0;
        _cleanUp = true;
        _setup = false;
        MovieEnd = true;
    }
}