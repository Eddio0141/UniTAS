using System;
using System.Collections.Generic;
using Sprache;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ParseInterfaces;
using P = Sprache.Parse;

namespace UniTASPlugin.Movie.DefaultParsers;

public class DefaultMovieScriptParser : IMovieScriptParser
{
    private static readonly Parser<int> Variable = P.Char('$')

    public IEnumerable<ScriptMethodModel> Parse(string input)
    {
        throw new NotImplementedException();
    }

    private enum TokenType
    {
        Loop,
        LeftCurlyBracket,
        RightCurlyBracket,
    }
}