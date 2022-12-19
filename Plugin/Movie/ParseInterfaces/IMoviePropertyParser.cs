using UniTASPlugin.Movie.MovieModels.Properties;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}