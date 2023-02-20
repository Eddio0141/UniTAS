using UniTAS.Plugin.Movie.MovieModels;

namespace UniTAS.Plugin.Movie.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}