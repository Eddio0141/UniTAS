using UniTASPlugin.Movie.MovieRunner.MovieModels.Properties;
using UniTASPlugin.Movie.MovieRunner.MovieModels.Script;

namespace UniTASPlugin.Movie.MovieRunner.MovieModels;

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