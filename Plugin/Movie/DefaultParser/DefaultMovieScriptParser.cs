using System.Collections.Generic;
using UniTASPlugin.Movie.Script;
using UniTASPlugin.MovieEngine.OpCodes;

namespace UniTASPlugin.Movie.DefaultParser;

public class DefaultMovieScriptParser : IMovieScriptParser
{
    public IEnumerable<KeyValuePair<string, IEnumerable<OpCodeBase>>> Parse(string input)
    {
        throw new System.NotImplementedException();
    }
}