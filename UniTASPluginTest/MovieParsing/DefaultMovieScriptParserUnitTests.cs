using Antlr4.Runtime;

namespace UniTASPluginTest.MovieParsing;

public class DefaultMovieScriptParserUnitTests
{
    private MovieScriptDefaultGrammarParser Setup(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        return speakParser;
    }

    [Fact]
    public void VariableAssignTypes()
    {
        var parser = Setup(@"$value = 10
$value2 = 10.0
$value3 = ""hello world!""
$value5 = true
$value6 = false");
    }

    [Fact]
    public void AssignTupleOrExpression()
    {
        var parser = Setup(@"$value = (10 + 5) * 3
$value2 = (10, 5, 3)
$value3 = (3 / (10 * 5), !(true && false))
$value4 = ((""nested"", 5), ((1.0, -2), ""nested 2"", (10 + 5) * (3)))
")
    }
}