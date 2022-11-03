using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using UniTASPlugin.Movie.DefaultParsers.DefaultMovieScriptParser;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.OpCodes;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.Method;
using UniTASPlugin.Movie.ScriptEngine.OpCodes.VariableSet;

namespace UniTASPluginTest.MovieParsing;

public class DefaultMovieScriptParserUnitTests
{
    private static ScriptModel Setup(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        var program = speakParser.program();
        var listener = new DefaultGrammarListenerCompiler();
        ParseTreeWalker.Default.Walk(listener, program);
        return listener.Compile();
    }

    [Fact]
    public void VariableAssignTypes()
    {
        var script = Setup(@"$value = 10
$value2 = 10.0
$value3 = ""hello world!""
$value5 = true
$value6 = false");
    }

    [Fact]
    public void AssignTupleOrExpression()
    {
        var script = Setup(@"$value = (10 + 5) * 3
$value2 = (10, 5, 3)
$value3 = (3 / (10 * 5), !(true && false))
$value4 = ((""nested"", 5), ((1.0, -2), ""nested 2"", (10 + 5) * (3)))
");
    }

    [Fact]
    public void NewMethod()
    {
        var script = Setup("fn method(arg1, arg2) { }");
        var definedMethod = script.Methods[0];

        var actual = new ScriptMethodModel("method",
            new List<OpCodeBase>
        {
            new PopArgOpCode(RegisterType.Temp),
            new NewVariableOpCode(RegisterType.Temp, "arg2"),
            new PopArgOpCode(RegisterType.Temp),
            new NewVariableOpCode(RegisterType.Temp, "arg1")
        });

        definedMethod.Should().BeEquivalentTo(actual);
    }
}