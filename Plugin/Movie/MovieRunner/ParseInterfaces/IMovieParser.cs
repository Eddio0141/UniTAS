using UniTASPlugin.Movie.ScriptEngine.MovieModels;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}