using System.Collections.Generic;
using UniTASPlugin.Movie.Script.LowLevel.OpCodes;

namespace UniTASPlugin.Movie.Script;

public interface IMovieScriptParser
{
    IEnumerable<OpCodeBase> Parse(string input);
}