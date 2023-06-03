namespace UniTAS.Plugin.Tests.MovieRunner;

public class ScriptOptionTests
{
    [Fact]
    public void DebugPrint()
    {
        const string input = @"
print(""test"")
i = 0
print(i)
";

        var (movieEngine, _, kernel) = Utils.Setup(input);
        var logger = kernel.GetInstance<KernelUtils.DummyLogger>();

        movieEngine.Update();

        Assert.Equal("test", logger.Infos[0]);
        Assert.Equal("0", logger.Infos[1]);
    }
}