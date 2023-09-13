using UniTAS.Patcher.Models.Movie;

namespace UniTAS.Patcher.Services.Movie;

public interface IMovieParser
{
    /// <summary>
    /// Parses the input into a movie engine.
    /// Script itself also contains properties that can be used to get information about the movie, which is also returned.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    (IMovieEngine, PropertiesModel) Parse(string input);
}