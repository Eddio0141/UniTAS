using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.Parsers.Exception;

namespace UniTAS.Plugin.Movie.Parsers.EngineParser;

public class MovieEngineParser : IMovieEngineParser
{
    public IMovieEngine Parse(string input)
    {
        var script = new Script();
        AddAliases(script);

        var wrappedInput = WrapInput(input);
        var engine = script.DoString(wrappedInput);

        // yield once to see if we are using global scope
        engine = script.CreateCoroutine(engine);
        engine.Coroutine.Resume();

        var globalScope = GlobalScope(script);
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

        return new MovieEngine(engine);
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
}