using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace UniTASPlugin.Movie.Parsers.MovieScriptParser;

public class ErrorListener : BaseErrorListener
{
    public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line,
        int charPositionInLine,
        string msg, RecognitionException e)
    {
        throw new ParseCanceledException($"line {line}:{charPositionInLine} {msg}");
    }
}