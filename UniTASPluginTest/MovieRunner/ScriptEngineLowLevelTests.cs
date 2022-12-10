using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.LowLevelEngine;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPluginTest.MovieRunner;

public class ScriptEngineLowLevelTests
{
    public class TestExternGetArgs : EngineExternalMethod
    {
        public List<string> Args { get; } = new();

        public TestExternGetArgs() : base("get_args", -1)
        {
        }

        public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args, ScriptEngineMovieRunner runner)
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

    private static ScriptEngineLowLevelEngine Setup(string input,
        IEnumerable<EngineExternalMethod> getDefinedMethods)
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
        return new(script, externMethods);
    }

    [Fact]
    public void TestInvoke()
    {
        var externMethod = new TestExternGetArgs();
        var engine = Setup(@"$loop_index = 0
loop 10 {
    get_args($loop_index)
    $loop_index += 1
}", new EngineExternalMethod[] { externMethod });

        engine.ExecUntilStop(null);
        externMethod.Args.Should().BeEquivalentTo("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
    }

    [Fact]
    public void TestTuple()
    {
        var externGetArgs = new TestExternGetArgs();
        var engine = Setup(@"$tuple = (59, ""hello world"", false, -10.3)
$(var, var2, var3, var4) = $tuple
get_args($var) | get_args($var2) | get_args($var3) | get_args($var4)
$(var, var2, var3, var4) = (""a"", ""b"", ""c"", ""d"")
get_args($var) | get_args($var2) | get_args($var3) | get_args($var4)",
            new EngineExternalMethod[] { externGetArgs });

        engine.ExecUntilStop(null);
        externGetArgs.Args.Should()
            .BeEquivalentTo("59", "hello world", "False", "-10.3", "a", "b", "c", "d");
    }

    [Fact]
    public void TestReturn()
    {
        var externGetArgs = new TestExternGetArgs();
        var engine = Setup(
            @"fn test_return() {
    return (10, 20)
}
fn test_return2() {
    return 99
}
fn test_return3(arg1, arg2) {
    return ($arg1, $arg2)
}

$(var, var2) = test_return()
get_args($var) | get_args($var2)

$var = test_return2()
get_args($var)

$(_, var) = test_return()
get_args($var)

$(var, var2) = test_return3(-10, -20)
get_args($var) | get_args($var2)
get_args($var, $var2)
",
            new EngineExternalMethod[] { externGetArgs });

        engine.ExecUntilStop(null);
        externGetArgs.Args.Should()
            .BeEquivalentTo("10", "20", "99", "20", "-10", "-20", "-10", "-20");
    }

    [Fact]
    public void FrameAdvance()
    {
        var engine = Setup("loop 5 { ; } ;", new EngineExternalMethod[] { });

        for (var i = 0; i < 5; i++)
        {
            engine.ExecUntilStop(null);
        }

        engine.FinishedExecuting.Should().BeFalse();
        engine.ExecUntilStop(null);
        engine.FinishedExecuting.Should().BeTrue();
    }

    [Fact]
    public void TestArgs()
    {
        var externGetArgs = new TestExternGetArgs();
        var engine = Setup(
            @"
fn test_args(arg, arg2) {
    get_args($arg)
    get_args($arg2)
}
$(value1, value2) = (10, 20)
get_args($value1) | get_args($value2)
get_args($value1, $value2)
get_args(""hello world"", false)

test_args(""hello world"", false)

get_args((10, 20), ""third item"", (40, 50, 60))",
            new EngineExternalMethod[] { externGetArgs });

        engine.ExecUntilStop(null);
        externGetArgs.Args.Should()
            .ContainInOrder("10", "20", "10", "20", "hello world", "False", "hello world", "False", "10", "20",
                "third item", "40", "50", "60");
    }
}