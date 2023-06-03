using System;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Exceptions.Movie.Engine;
using UniTAS.Patcher.Services.Movie;

namespace UniTAS.Patcher.Implementations.Movie.Engine;

public partial class MovieEngine
{
    /// <summary>
    /// A wrapper for a coroutine that will run indefinitely
    /// </summary>
    private class CoroutineHolder
    {
        private readonly IMovieEngine _engine;
        private readonly DynValue _method;
        private DynValue _coroutine;
        private readonly DynValue[] _defaultArgs;
        public bool RunOnce { get; }
        public bool Finished => _coroutine.Coroutine.State == CoroutineState.Dead;

        public CoroutineHolder(IMovieEngine engine, DynValue method, DynValue[] defaultArgs, bool runOnce)
        {
            _engine = engine;
            _method = method;
            _defaultArgs = defaultArgs;
            RunOnce = runOnce;
            InitCoroutine();
        }

        public void Resume()
        {
            if (_coroutine.Coroutine.State == CoroutineState.Dead)
            {
                InitCoroutine();
            }

            try
            {
                _coroutine.Coroutine.Resume(_defaultArgs);
            }
            catch (Exception)
            {
                throw new CoroutineResumeException(
                    "Failed to resume coroutine, check your lua code, there is an runtime error");
            }
        }

        private void InitCoroutine()
        {
            _coroutine = _engine.Script.CreateCoroutine(_method);
        }
    }
}