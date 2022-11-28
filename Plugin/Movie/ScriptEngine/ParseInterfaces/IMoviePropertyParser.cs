using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Properties;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}