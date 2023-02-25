using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.EngineParser;

public interface IMovieEngineParser
{
    IMovieEngine Parse(string input);
}