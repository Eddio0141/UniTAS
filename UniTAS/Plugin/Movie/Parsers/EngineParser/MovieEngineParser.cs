using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.EngineParser;

public class MovieEngineParser : IMovieEngineParser
{
    public IMovieEngine Parse(string input)
    {
        // wrap input in function for coroutine
        input = $"return function() {input} end";
        var script = new Script();

        var engine = script.DoString(input);
        engine = script.CreateCoroutine(engine);

        return new MovieEngine(engine);
    }
}