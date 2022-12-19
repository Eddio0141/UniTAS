using UniTASPlugin.Movie.MovieRunner.MovieModels.Properties;

namespace UniTASPlugin.Movie.MovieRunner.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}