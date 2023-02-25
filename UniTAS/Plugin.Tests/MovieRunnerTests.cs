using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Movie.Engine;
using UniTAS.Plugin.Movie.Parsers.EngineParser;

namespace UniTAS.Plugin.Tests;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
public class MovieRunnerTests
{
    private static IMovieEngine Setup(string input)
    {
        var kernel = ContainerRegister.Init();

        var parser = kernel.GetInstance<IMovieEngineParser>();
        return parser.Parse(input);
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
        var movieRunner = Setup(input);

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }
}