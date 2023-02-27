using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.Exception;
using UniTAS.Plugin.Movie.Parsers.MovieParser;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public class MovieRunnerTests
{
    private static (IMovieEngine, PropertiesModel) Setup(string input)
    {
        var kernel = ContainerRegister.Init();

        var parser = kernel.GetInstance<IMovieParser>();
        var parsed = parser.Parse(input);

        return (parsed.Item1, parsed.Item2);
    }

    [Fact]
    public void MainConcurrent()
    {
        const string input = @"
i = 0
coroutine.yield()
i = i + 1
coroutine.yield()
i = i + 1
";
        var movieRunner = Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void CoroutineWithoutYield()
    {
        const string input = @"
i = 0
i = i + 1
";
        var movieRunner = Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void CoroutineAlias()
    {
        const string input = @"
i = 0
adv()
i = i + 1
";

        var movieRunner = Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void GlobalScopeInvalidYield()
    {
        const string input = @"
USE_GLOBAL_SCOPE = true
-- try use yield which should not be available
adv()
";

        Assert.Throws<ScriptRuntimeException>(() => Setup(input));
    }

    [Fact]
    public void GlobalScope()
    {
        const string input = @"
USE_GLOBAL_SCOPE = true
return function()
    i = 0
    adv()
    i = i + 1
end
";

        var movieRunner = Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void GlobalScopeInvalidNoReturn()
    {
        // don't return a function
        const string input = @"
USE_GLOBAL_SCOPE = true
i = 0
i = i + 1
";

        Assert.Throws<NotReturningFunctionException>(() => Setup(input));
    }

    [Fact]
    public void PropertiesFull()
    {
        const string input = @"
START_TIME = ""28/03/2021 12:00:00""
frametime = 1/60
";

        var properties = Setup(input).Item2;

        Assert.Equal(new(2021, 03, 28, 12, 00, 00), properties.StartupProperties.StartTime);
        Assert.Equal(1 / 60f, properties.StartupProperties.FrameTime);
    }
}