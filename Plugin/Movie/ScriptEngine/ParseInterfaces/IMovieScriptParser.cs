using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Script;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}