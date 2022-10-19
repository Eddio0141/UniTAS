using UniTASPlugin.Movie.Models.Properties;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}