using UniTASPlugin.Movie.Models;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMovieParser
{
    MovieModel Parse(string input);
}