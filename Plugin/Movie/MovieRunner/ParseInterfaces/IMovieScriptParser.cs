using System.Collections.Generic;
using UniTASPlugin.Movie.MovieRunner.MovieModels.Script;

namespace UniTASPlugin.Movie.MovieRunner.ParseInterfaces;

public interface IMovieScriptParser
{
    IEnumerable<ScriptMethodModel> Parse(string input);
}