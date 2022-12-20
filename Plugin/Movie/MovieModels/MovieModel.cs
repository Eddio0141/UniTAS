using UniTASPlugin.Movie.MovieModels.Properties;
using UniTASPlugin.Movie.MovieModels.Script;

namespace UniTASPlugin.Movie.MovieModels;

public class MovieModel
{
    public PropertiesModel Properties { get; }
    public ScriptModel Script { get; }

    public MovieModel(PropertiesModel properties, ScriptModel script)
    {
        Properties = properties;
        Script = script;
    }
}