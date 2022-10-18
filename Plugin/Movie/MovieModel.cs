using UniTASPlugin.Movie.Properties;
using UniTASPlugin.Movie.Script;

namespace UniTASPlugin.Movie;

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