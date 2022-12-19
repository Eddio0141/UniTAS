using UniTASPlugin.Movie.MovieModels;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}