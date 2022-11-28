using UniTASPlugin.Movie.ScriptEngine.Models;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}