using FluentAssertions;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.Movie.ScriptEngine;
using UniTASPlugin.Movie.ScriptEngine.EngineMethods;
using UniTASPlugin.Movie.ScriptEngine.Parsers;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;
using UniTASPlugin.Movie.ScriptEngine.Parsers.MovieScriptParser;

namespace UniTASPluginTest.MovieRunner;

public class MovieRunnerTests
{
    private class FakeVEnvService : IVirtualEnvironmentService
    {
        public VirtualEnvironment GetVirtualEnv()
        {
            return new();
        }
    }

    private static ScriptEngineMovieRunner Setup(IEnumerable<EngineExternalMethod> getDefinedMethods)
    {
        var externMethods = getDefinedMethods.ToList();
        var runner = new ScriptEngineMovieRunner(
            new ScriptEngineMovieParser(new DefaultMovieSectionSplitter(), new DefaultMoviePropertiesParser(),
                new DefaultMovieScriptParser(externMethods)), externMethods, new FakeVEnvService());

        return runner;
    }

    [Fact]
    public void ConcurrentRunners()
    {
        var externGetArgs = new ScriptEngineLowLevelTests.TestExternGetArgs();

        var runner = Setup(new EngineExternalMethod[]
            { externGetArgs, new RegisterExternalMethod(), new UnregisterExternalMethod() });
        // ReSharper disable once StringLiteralTypo
        const string input = @"name test TAS
author yuu0141
desc a test TAS
os Windows
datetime 03/28/2002
ft 0.001
resolution 900 600
unfocused
fullscreen
endsave end_save
---
fn concurrent() {
    get_args(1);
    get_args(2)
}
fn concurrent2() {
    get_args(3);
    get_args(4);
    get_args(5)
}

get_args(""concurrent"", true)
$concurrent1 = register(""concurrent"", true) | $concurrent2 = register(""concurrent2"", false)
get_args(-1);
get_args(-2);
get_args(-3);
get_args(-4)
unregister($concurrent1, true);
get_args(-5)";
        runner.RunFromInput(input);

        runner.Update();
        runner.Update();
        runner.Update();
        runner.IsRunning.Should().BeTrue();
        runner.Update();
        runner.IsRunning.Should().BeTrue();
        runner.Update();

        externGetArgs.Args.Should()
            .ContainInOrder("concurrent", "True", "1", "-1", "3", "2", "-2", "4", "1", "-3", "5", "2", "-4", "3", "-5",
                "4");
        runner.IsRunning.Should().BeFalse();
    }
}