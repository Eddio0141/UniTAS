using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Utils;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public interface IMovieParser
{
    /// <summary>
    /// Parses the input into a movie engine.
    /// Script itself also contains properties that can be used to get information about the movie, which is also returned.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Tuple<IMovieEngine, PropertiesModel> Parse(string input);
}