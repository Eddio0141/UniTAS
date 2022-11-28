using UniTASPlugin.Movie.ScriptEngine.Models;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}