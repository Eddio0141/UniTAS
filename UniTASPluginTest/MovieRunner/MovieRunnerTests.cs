using FluentAssertions;
using UniTASPlugin.FixedUpdateSync;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.GameRestart;
using UniTASPlugin.Movie;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Movie.Parsers;
using UniTASPlugin.Movie.Parsers.MoviePropertyParser;
using UniTASPlugin.Movie.Parsers.MovieScriptParser;

namespace UniTASPluginTest.MovieRunner;

public class MovieRunnerTests
{
    private static UniTASPlugin.Movie.MovieRunner Setup(IEnumerable<EngineExternalMethod> getDefinedMethods)
    {
        var externMethods = getDefinedMethods.ToList();
        var runner = new UniTASPlugin.Movie.MovieRunner(
            new MovieParser(new MovieSectionSplitter(), new MoviePropertyParser(),
                new MovieScriptParser(externMethods)), externMethods, new FakeVEnvFactory(),
            new FakeRestartService(), new FakeFixedUpdateService());

        return runner;
    }

    private class FakeFixedUpdateService : ISyncFixedUpdate
    {
        public void OnSync(Action callback, uint syncOffset = 0, ulong cycleOffset = 0)
        {
            callback();
        }
    }

    private class FakeVEnvFactory : IVirtualEnvironmentFactory
    {
        public VirtualEnvironment GetVirtualEnv()
        {
            return new();
        }
    }

    private class FakeRestartService : IGameRestart
    {
        public bool PendingRestart => false;

        public void SoftRestart(DateTime time)
        {
        }
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

    [Fact]
    public void MultipleFrameAdvance()
    {
        var externGetArgs = new ScriptEngineLowLevelTests.TestExternGetArgs();

        var runner = Setup(new EngineExternalMethod[] { externGetArgs });
        // ReSharper disable once StringLiteralTypo
        const string input = @"name test TAS
author yuu0141
desc a test TAS
os Windows
datetime 03/28/2002
ft 0.01
resolution 900 600
unfocused
fullscreen
endsave end_save
---
loop 500 { ; }
get_args(""checkpoint 1"");
loop 500 { ; }
get_args(""checkpoint 2"")";
        runner.RunFromInput(input);

        for (var i = 0; i < 500; i++)
        {
            runner.Update();
        }

        runner.IsRunning.Should().BeTrue();

        externGetArgs.Args.Should().BeEmpty();
        runner.Update();
        externGetArgs.Args.Should().ContainInOrder("checkpoint 1");

        for (var i = 0; i < 501; i++)
        {
            runner.Update();
        }

        runner.IsRunning.Should().BeFalse();
        externGetArgs.Args.Should().ContainInOrder("checkpoint 1", "checkpoint 2");
    }

    [Fact]
    public void VariableAccess()
    {
        var externGetArgs = new ScriptEngineLowLevelTests.TestExternGetArgs();

        var runner = Setup(new EngineExternalMethod[] { externGetArgs, new RegisterExternalMethod() });
        // ReSharper disable once StringLiteralTypo
        const string input = @"name test TAS
author yuu0141
desc a test TAS
os Windows
datetime 03/28/2002
ft 0.01
resolution 900 600
unfocused
fullscreen
endsave end_save
---
fn test_access() {
    get_args($test_var)
}

fn test_access2() {
    $test_var = 2
}

$test_var = 0
test_access()
test_access2()
get_args($test_var)
register(""test_access"", true);
get_args($test_var)";
        runner.RunFromInput(input);
        runner.Update();
        runner.Update();

        externGetArgs.Args.Should().ContainInOrder("0", "2", "2", "2");
    }
}