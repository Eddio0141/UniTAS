namespace UniTASPlugin.Movie;

public interface IMovieParser
{
    MovieModel Parse(string input);
}