using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
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
        var scriptAndMovieEngine = SetupScript();
        var script = scriptAndMovieEngine.Item1;
        var movieEngine = scriptAndMovieEngine.Item2;

        var wrappedInput = WrapInput(input);
        var engineCoroutine = script.DoString(wrappedInput);

        // yield once to see if we are using global scope
        engineCoroutine = script.CreateCoroutine(engineCoroutine);
        engineCoroutine.Coroutine.Resume();

        var useGlobalScope = GlobalScopeFlag(script);
        var properties = ProcessProperties(script);
        if (useGlobalScope)
        {
            script = SetupScript(movieEngine).Item1;
            engineCoroutine = script.DoString(input);

            // because we are using global scope, we expect a function to be returned
            if (engineCoroutine.Type != DataType.Function)
            {
                throw new NotReturningFunctionException("Expected a function to be returned from the global scope");
            }
        }
        else
        {
            script = SetupScript(movieEngine).Item1;
            engineCoroutine = script.DoString(wrappedInput);
        }

        // reset the engine
        movieEngine.InitCoroutine(engineCoroutine);

        return new(movieEngine, properties);
    }

    private Tuple<Script, MovieEngine> SetupScript(MovieEngine movieEngine = null)
    {
        var script = new Script
        {
            Options =
            {
                // do NOT use unity loader
                ScriptLoader = new FileSystemScriptLoader()
            }
        };

        if (movieEngine == null)
        {
            script.Options.DebugPrint = _ => { };
        }
        else
        {
            script.Options.DebugPrint = s => _logger.LogInfo(s);
        }

        if (movieEngine == null)
        {
            movieEngine = new(script);
        }
        else
        {
            movieEngine.Script = script;
        }

        AddEngineMethods(movieEngine);
        AddCustomTypes();

        return Tuple.New(script, movieEngine);
    }

    private void AddEngineMethods(IMovieEngine engine)
    {
        AddEngineMethodGlobal(engine);

        var engineMethodClasses = _engineMethodClassesFactory.GetAll(engine);

        foreach (var methodClass in engineMethodClasses)
        {
            var descriptor = (StandardUserDataDescriptor)UserData.RegisterType(methodClass.GetType());
            descriptor.RemoveMember(nameof(EngineMethodClass.ClassName));

            engine.Script.Globals[methodClass.ClassName] = methodClass;
        }
    }

    private static void AddCustomTypes()
    {
        UserData.RegisterAssembly(typeof(MovieParser).Assembly);
    }

    private static string WrapInput(string input)
    {
        return $"return function() {input} end";
    }

    private static bool GlobalScopeFlag(Script script)
    {
        const string variable = "GLOBAL_SCOPE";

        var globalScope = script.Globals.Get(variable);

        return globalScope.Type == DataType.Boolean && globalScope.Boolean;
    }
}