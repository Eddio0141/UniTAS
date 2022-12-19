using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

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
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        speakParser.RemoveErrorListeners();
        speakParser.AddErrorListener(new ErrorListener());
        var script = speakParser.script();
        var listener = new DefaultGrammarListenerCompiler(_getDefinedMethods);
        ParseTreeWalker.Default.Walk(listener, script);
        return listener.Compile();
    }
}