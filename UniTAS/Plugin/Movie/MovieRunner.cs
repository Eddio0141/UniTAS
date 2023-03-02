using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.FixedUpdateSync;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.GameRestart;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.Exceptions;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.MovieParser;
using UniTAS.Plugin.Utils;

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

    private readonly IMovieParser _parser;
    private IMovieEngine _engine;
    private readonly IMovieLogger _movieLogger;

    public MovieRunner(VirtualEnvironment vEnv, IGameRestart gameRestart, ISyncFixedUpdate syncFixedUpdate,
        IMovieParser parser, IMovieLogger movieLogger)
    {
        _virtualEnvironment = vEnv;
        _gameRestart = gameRestart;
        _syncFixedUpdate = syncFixedUpdate;
        _parser = parser;
        _movieLogger = movieLogger;
    }

    public void RunFromInput(string input)
    {
        if (!MovieEnd || _setup) throw new MovieAlreadyRunningException();

        _setup = true;

        Tuple<IMovieEngine, PropertiesModel> parsed;
        try
        {
            parsed = _parser.Parse(input);
        }
        catch (Exception e)
        {
            _setup = false;

            _movieLogger.LogError($"Failed to run TAS movie, an exception was thrown!");
            _movieLogger.LogError(e.Message);
            Trace.Write(e);

            return;
        }

        _engine = parsed.Item1;
        var properties = parsed.Item2;

        // set env from properties
        _virtualEnvironment.RunVirtualEnvironment = true;

        if (properties.StartupProperties != null)
        {
            Trace.Write($"Using startup property: {properties.StartupProperties}");
            _virtualEnvironment.FrameTime = properties.StartupProperties.FrameTime;
            _gameRestart.SoftRestart(properties.StartupProperties.StartTime);
        }

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

        _engine.Update();

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