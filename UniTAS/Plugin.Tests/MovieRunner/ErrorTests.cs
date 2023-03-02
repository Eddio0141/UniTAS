namespace UniTAS.Plugin.Tests.MovieRunner;

public class ErrorTests
{
    [Fact]
    public void RuntimeError()
    {
        const string input = @"
adv()
i = j + k
";

        var (movieRunner, _, kernel) = Utils.Setup(input);
        var logger = kernel.GetInstance<Utils.DummyLogger>();

        movieRunner.Update();
        Assert.False(movieRunner.Finished);

        movieRunner.Update();
        Assert.True(movieRunner.Finished);

        Assert.Equal(2, logger.Errors.Count);
        Assert.Equal("Movie threw a runtime exception!", logger.Errors[0]);
    }
}