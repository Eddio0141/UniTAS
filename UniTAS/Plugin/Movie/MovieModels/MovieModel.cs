using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.MovieModels.Script;

namespace UniTAS.Plugin.Movie.MovieModels;

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