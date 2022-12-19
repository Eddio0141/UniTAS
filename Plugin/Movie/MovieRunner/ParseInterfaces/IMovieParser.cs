using UniTASPlugin.Movie.MovieRunner.MovieModels;

namespace UniTASPlugin.Movie.MovieRunner.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}