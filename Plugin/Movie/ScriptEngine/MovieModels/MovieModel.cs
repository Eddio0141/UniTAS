using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Properties;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Script;

namespace UniTASPlugin.Movie.ScriptEngine.Models.Movie;

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