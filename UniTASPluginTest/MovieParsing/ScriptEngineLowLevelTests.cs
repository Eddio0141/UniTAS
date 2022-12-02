using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.LowLevelEngine;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using UniTASPlugin.Movie.ScriptEngine.ValueTypes;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPluginTest.MovieParsing;

public class ScriptEngineLowLevelTests
{
    private class TestExternGetArgs : EngineExternalMethodBase
    {
        public List<string> Args { get; } = new();

        public TestExternGetArgs() : base("get_args", -1)
        {
        }

        public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args)
        {
            foreach (var argTuple in args)
            {
                foreach (var arg in argTuple)
                {
                    Args.Add(arg.ToString());
                }
            }

            return new();
        }
    }

    private class TestExternReturnValues : EngineExternalMethodBase
    {
        public TestExternReturnValues() : base("return_values", argReturnCount: 4)
        {
        }

        public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args)
        {
            return new()
            {
                new IntValueType(2), new StringValueType("testing stuff!"), new BoolValueType(false),
                new FloatValueType(-10.29f)
            };
        }
    }

    private static ScriptEngineLowLevelEngine Setup(string input,
        IEnumerable<EngineExternalMethodBase> getDefinedMethods)
    {
        var externMethods = getDefinedMethods.ToList();

        var inputStream = new AntlrInputStream(input);
        var speakLexer = new MovieScriptDefaultGrammarLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(speakLexer);
        var speakParser = new MovieScriptDefaultGrammarParser(commonTokenStream);
        var program = speakParser.script();
        var listener = new DefaultGrammarListenerCompiler(externMethods);
        ParseTreeWalker.Default.Walk(listener, program);
        var methods = listener.Compile().ToList();
        var mainMethod = methods.First(x => x.Name == null);
        var definedMethods = methods.Where(x => x.Name != null);
        var script = new ScriptModel(mainMethod, definedMethods);
        return new ScriptEngineLowLevelEngine(script, externMethods);
    }

    [Fact]
    public void TestInvoke()
    {
        var externMethod = new TestExternGetArgs();
        var engine = Setup(@"$loop_index = 0
loop 10 {
    get_args($loop_index)
    $loop_index += 1
}", new EngineExternalMethodBase[] { externMethod });

        engine.ExecUntilStop();
        externMethod.Args.Should().BeEquivalentTo("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
    }

    [Fact]
    public void TestTuple()
    {
        var externGetArgs = new TestExternGetArgs();
        var engine = Setup(
@"$tuple = (59, ""hello world"", false, -10.3)
$(var, var2, var3, var4) = $tuple
get_args($var) | get_args($var2) | get_args($var3) | get_args($var4)
$(var, var2, var3, var4) = (""a"", ""b"", ""c"", ""d"")
get_args($var, $var2, $var3, $var4)",
            new EngineExternalMethodBase[] { externGetArgs });

        engine.ExecUntilStop();
        externGetArgs.Args.Should()
            .BeEquivalentTo(new[] { "59", "hello world", "false", "-10.3", "a", "b", "c", "d" });
    }
}