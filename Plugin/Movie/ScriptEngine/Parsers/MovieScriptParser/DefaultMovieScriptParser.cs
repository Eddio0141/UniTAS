using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

public class DefaultMovieScriptParser : IMovieScriptParser
{
    private readonly IGetDefinedMethods _getDefinedMethods;

    public DefaultMovieScriptParser(IGetDefinedMethods getDefinedMethods)
    {
        _getDefinedMethods = getDefinedMethods;
    }

    public IEnumerable<ScriptMethodModel> Parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        var program = speakParser.program();
        var listener = new DefaultGrammarListenerCompiler(_getDefinedMethods);
        ParseTreeWalker.Default.Walk(listener, program);
        return listener.Compile();
    }
}