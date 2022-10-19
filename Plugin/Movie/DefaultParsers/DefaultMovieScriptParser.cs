using System.Collections.Generic;
using UniTASPlugin.Movie.ParseInterfaces;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;

namespace UniTASPlugin.Movie.DefaultParsers;

public class DefaultMovieScriptParser : IMovieScriptParser
{
    public IEnumerable<KeyValuePair<string, IEnumerable<OpCodeBase>>> Parse(string input)
    {
        throw new System.NotImplementedException();
    }
}