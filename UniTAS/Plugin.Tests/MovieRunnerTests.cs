using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using MoonSharp.Interpreter;
using StructureMap;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.MovieModels.Properties;
using UniTAS.Plugin.Movie.Parsers.Exceptions;
using UniTAS.Plugin.Movie.Parsers.MovieParser;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public class MovieRunnerTests
{
    private class DummyLogger : IMovieLogger
    {
        public List<string> Warns { get; } = new();

        public void LogError(object data)
        {
        }

        public void LogInfo(object data)
        {
        }

        public void LogWarning(object data)
        {
            Warns.Add(data.ToString() ?? string.Empty);
        }

        public event EventHandler<LogEventArgs>? OnLog;
    }

    private static (IMovieEngine, PropertiesModel, IContainer) Setup(string input)
    {
        var kernel = ContainerRegister.Init();

        kernel.Configure(x =>
        {
            x.ForSingletonOf<DummyLogger>().Use<DummyLogger>();
            x.For<IMovieLogger>().Use(y => y.GetInstance<DummyLogger>());
        });

        var parser = kernel.GetInstance<IMovieParser>();
        var parsed = parser.Parse(input);

        return (parsed.Item1, parsed.Item2, kernel);
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

    [Fact]
    public void FpsFrametimeConflict()
    {
        const string input = @"
START_TIME = ""28/03/2021 12:00:00""
fps = 60
frametime = 1/50
";

        var (_, properties, kernel) = Setup(input);

        Assert.Equal(1 / 50f, properties.StartupProperties.FrameTime);
        Assert.Equal("frametime and fps are both defined, using frametime", kernel.GetInstance<DummyLogger>().Warns[0]);
    }

    [Fact]
    public void MissingStartTime()
    {
        const string input = @"
fps = 60
";

        var (_, _, kernel) = Setup(input);

        Assert.Equal("START_TIME is not defined, using default value of 01/01/0001 00:00:00",
            kernel.GetInstance<DummyLogger>().Warns[0]);
    }

    [Fact]
    public void ConcurrentPreUpdate()
    {
        const string input = @"
i = 0
j = 0

function preUpdate()
    i = i + 1
    adv()
    i = i + 2
end

concurrent.register(preUpdate, true)

j = j + 1
adv()
j = j + 2
adv()
j = j + 3
adv()
j = j + 4
";

        var movieRunner = Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }
}