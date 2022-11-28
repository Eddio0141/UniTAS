using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.Models.Script;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}