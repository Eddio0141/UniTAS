namespace UniTASPlugin.Movie.Properties;

public interface IMoviePropertyParser
{
    PropertiesModel Parse(string input);
}