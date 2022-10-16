using System.Collections.Generic;
using UniTASPlugin.MovieEngine.OpCodes;

namespace UniTASPlugin.Movie.Script;

public interface IMovieScriptParser
{
    IEnumerable<OpCodeBase> Parse(string input);
}