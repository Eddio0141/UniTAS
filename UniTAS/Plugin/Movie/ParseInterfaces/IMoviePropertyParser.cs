using UniTAS.Plugin.Movie.MovieModels.Properties;

namespace UniTAS.Plugin.Movie.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}