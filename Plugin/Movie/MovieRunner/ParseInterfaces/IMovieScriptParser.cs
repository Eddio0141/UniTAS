using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}