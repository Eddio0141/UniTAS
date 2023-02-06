using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Movie.MovieModels.Script;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.Parsers.MovieScriptParser;

public class MovieScriptParser : IMovieScriptParser
{
    private readonly EngineExternalMethod[] _getDefinedMethods;

    public MovieScriptParser(IEnumerable<EngineExternalMethod> externMethods)
    {
        _getDefinedMethods = externMethods.ToArray();
    }

    public IEnumerable<ScriptMethodModel> Parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieGrammarParser(commonTokenStream);
        speakParser.RemoveErrorListeners();
        speakParser.AddErrorListener(new ErrorListener());
        var script = speakParser.script();
        var listener = new ScriptCompiler(_getDefinedMethods);
        ParseTreeWalker.Default.Walk(listener, script);
        return listener.Compile();
    }
}