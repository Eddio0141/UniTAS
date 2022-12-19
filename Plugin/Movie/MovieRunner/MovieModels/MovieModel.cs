using UniTASPlugin.Movie.ScriptEngine.MovieModels.Properties;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;

namespace UniTASPlugin.Movie.ScriptEngine.MovieModels;

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