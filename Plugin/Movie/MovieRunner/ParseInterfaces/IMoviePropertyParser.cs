using UniTASPlugin.Movie.ScriptEngine.MovieModels.Properties;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}