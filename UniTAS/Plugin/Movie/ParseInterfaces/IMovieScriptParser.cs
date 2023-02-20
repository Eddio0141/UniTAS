using System.Collections.Generic;
using UniTAS.Plugin.Movie.MovieModels.Script;

namespace UniTAS.Plugin.Movie.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}