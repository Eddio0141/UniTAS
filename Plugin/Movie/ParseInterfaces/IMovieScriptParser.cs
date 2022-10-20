using System.Collections.Generic;
using UniTASPlugin.Movie.Models.Script;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}