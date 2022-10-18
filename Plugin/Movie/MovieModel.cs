using Sprache;
using UniTASPlugin.Movie.Properties;
using UniTASPlugin.Movie.Script;

namespace UniTASPlugin.Movie;

public class MovieModel
{
    public PropertiesModel Properties { get; }
    public ScriptModel Script { get; }

    private readonly Parser<string> propertyParser = ;

    public MovieModel ParseFromText(string input, IMoviePropertyParser propertyParser, IMovieScriptParser scriptParser)
    {

    }
}