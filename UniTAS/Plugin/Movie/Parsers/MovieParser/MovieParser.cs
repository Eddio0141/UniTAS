using System;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.Exception;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public class MovieParser : IMovieParser
{
    public Tuple<IMovieEngine, PropertiesModel> Parse(string input)
    {
        var script = new Script();
        AddAliases(script);

        var wrappedInput = WrapInput(input);
        var engine = script.DoString(wrappedInput);

        // yield once to see if we are using global scope
        engine = script.CreateCoroutine(engine);
        engine.Coroutine.Resume();

        var globalScope = GlobalScope(script);
        var properties = ProcessProperties(script);
        if (globalScope)
        {
            engine = script.DoString(input);

            // because we are using global scope, we expect a function to be returned
            if (engine.Type != DataType.Function)
            {
                throw new NotReturningFunctionException("Expected a function to be returned from the global scope");
            }
        }
        else
        {
            engine = script.DoString(wrappedInput);
        }

        // reset the engine
        engine = script.CreateCoroutine(engine);

        return new(new MovieEngine(engine), properties);
    }

    private static void AddAliases(Script script)
    {
        // alias method to coroutine.yield as adv
        var yield = script.Globals.Get("coroutine").Table.Get("yield");
        script.Globals.Set("adv", yield);
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

    private static PropertiesModel ProcessProperties(Script script)
    {
        // var os = script.Globals.Get("GAME_OS");
        var startTime = script.Globals.Get("START_TIME");
        // var fps = script.Globals.Get("fps");
        var frameTime = script.Globals.Get("frametime");

        var properties = new PropertiesModel(new(
            startTime.Type == DataType.String ? DateTime.Parse(startTime.String) : default,
            frameTime.Type == DataType.Number ? (float)frameTime.Number : default
        ));

        return properties;
    }
}