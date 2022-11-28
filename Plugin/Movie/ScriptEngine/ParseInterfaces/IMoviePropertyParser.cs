using UniTASPlugin.Movie.ScriptEngine.Models.Properties;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}