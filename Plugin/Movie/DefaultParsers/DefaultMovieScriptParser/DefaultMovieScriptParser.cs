using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;

public partial class DefaultMovieScriptParser : IMovieScriptParser
{
    public IEnumerable<ScriptMethodModel> Parse(string input)
    {
        throw new NotImplementedException();
    }
}