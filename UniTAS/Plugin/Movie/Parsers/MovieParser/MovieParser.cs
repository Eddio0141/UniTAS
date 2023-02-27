using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.Exceptions;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public partial class MovieParser : IMovieParser
{
    private readonly IMovieLogger _logger;

    private readonly IEngineMethodClassesFactory _engineMethodClassesFactory;

    public MovieParser(IMovieLogger logger, IEngineMethodClassesFactory engineMethodClassesFactory)
    {
        _logger = logger;
        _engineMethodClassesFactory = engineMethodClassesFactory;
    }

    public Tuple<IMovieEngine, PropertiesModel> Parse(string input)
    {
        var script = new Script
        {
            Options =
            {
                DebugPrint = s => _logger.LogInfo(s),
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader()
            }
        };

        var movieEngine = new MovieEngine(script);

        AddAliases(script);
        AddEngineMethods(movieEngine);

        var wrappedInput = WrapInput(input);
        var engineCoroutine = script.DoString(wrappedInput);

        // yield once to see if we are using global scope
        engineCoroutine = script.CreateCoroutine(engineCoroutine);
        engineCoroutine.Coroutine.Resume();

        var globalScope = GlobalScope(script);
        var properties = ProcessProperties(script);
        if (globalScope)
        {
            engineCoroutine = script.DoString(input);

            // because we are using global scope, we expect a function to be returned
            if (engineCoroutine.Type != DataType.Function)
            {
                throw new NotReturningFunctionException("Expected a function to be returned from the global scope");
            }
        }
        else
        {
            engineCoroutine = script.DoString(wrappedInput);
        }

        // reset the engine
        movieEngine.InitCoroutine(engineCoroutine);

        return new(movieEngine, properties);
    }

    private static void AddAliases(Script script)
    {
        // alias method to coroutine.yield as adv
        var yield = script.Globals.Get("coroutine").Table.Get("yield");
        script.Globals.Set("adv", yield);
    }

    private void AddEngineMethods(IMovieEngine engine)
    {
        var engineMethodClasses = _engineMethodClassesFactory.GetAll(engine);

        foreach (var methodClass in engineMethodClasses)
        {
            UserData.RegisterType(methodClass.GetType());
            engine.Script.Globals[methodClass.ClassName] = methodClass;
        }
    }

    private static string WrapInput(string input)
    {
        return $"return function() {input} end";
    }

    private static bool GlobalScope(Script script)
    {
        const string variable = "USE_GLOBAL_SCOPE";

        var globalScope = script.Globals.Get(variable);

        return globalScope.Type == DataType.Boolean && globalScope.Boolean;
    }
}