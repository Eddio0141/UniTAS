using UniTASPlugin.Movie.Models.Properties;
using UniTASPlugin.Movie.Models.Script;

namespace UniTASPlugin.Movie.Models;

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