using UniTASPlugin.Movie.ScriptEngine.Models.Properties;
using UniTASPlugin.Movie.ScriptEngine.Models.Script;

namespace UniTASPlugin.Movie.ScriptEngine.Models;

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