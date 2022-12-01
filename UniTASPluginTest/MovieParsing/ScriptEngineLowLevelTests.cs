using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FluentAssertions;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.LowLevelEngine;
using UniTASPlugin.Movie.ScriptEngine.MovieModels.Script;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;
using ValueType = UniTASPlugin.Movie.ScriptEngine.ValueTypes.ValueType;

namespace UniTASPluginTest.MovieParsing;

public class ScriptEngineLowLevelTests
{
    private interface ITestInvoke
    {
        List<string> UpdateArgsList();
    }

    private class TestThing : ITestInvoke
    {
        public List<string> ArgTestList { get; } = new();

        public List<string> UpdateArgsList()
        {
            return ArgTestList;
        }
    }

    private class TestExternMethod : EngineExternalMethodBase
    {
        private readonly ITestInvoke _testInvoke;

        public TestExternMethod(ITestInvoke testInvoke) : base("test", 1)
        {
            _testInvoke = testInvoke;
        }

        public override List<ValueType> Invoke(IEnumerable<IEnumerable<ValueType>> args)
        {
            foreach (var argTuple in args)
            {
                _testInvoke.UpdateArgsList().Add(argTuple.First().ToString() ?? throw new InvalidOperationException());
            }

            return new();
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
        var testList = new TestThing();
        var externMethod = new TestExternMethod(testList);

        var engine = Setup(@"$loop_index = 0
loop 10 {
    test($loop_index)
    $loop_index += 1
}", new EngineExternalMethodBase[] { externMethod });

        engine.ExecUntilStop();

        testList.ArgTestList.Should().BeEquivalentTo("0", "1", "2", "3", "4", "5", "6", "7", "8", "9");
    }
}