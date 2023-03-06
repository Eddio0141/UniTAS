namespace UniTAS.Plugin.Tests.MovieRunner;

public class ConfigTests
{
    [Fact]
    public void ConfigFull()
    {
        const string input = @"
MOVIE_CONFIG = {
    is_global_scope = false,
    frametime = 1/60,
    start_time = ""03/28/2021 12:00:00""
}
";

        var properties = Utils.Setup(input).Item2;

        // 2021-03-28T12:00:00.0000000
        Assert.Equal(new(2021, 3, 28, 12, 0, 0, DateTimeKind.Utc), properties.StartupProperties.StartTime);
        Assert.Equal(1 / 60f, properties.StartupProperties.FrameTime);
    }

    [Fact]
    public void FpsFrametimeConflict()
    {
        const string input = @"
MOVIE_CONFIG = {
    frametime = 1/50,
    start_time = ""03/28/2021 12:00:00"",
    fps = 60
}
";

        var (_, properties, kernel) = Utils.Setup(input);

        Assert.Equal(1 / 50f, properties.StartupProperties.FrameTime);
        Assert.Equal("frametime and fps are both defined, using frametime",
            kernel.GetInstance<KernelUtils.DummyLogger>().Warns[0]);
    }

    [Fact]
    public void MissingStartTime()
    {
        const string input = @"
MOVIE_CONFIG = {
    fps = 60
}
";

        var (_, _, kernel) = Utils.Setup(input);

        Assert.Equal("start_time is not defined, using default value of 01/01/0001 00:00:00",
            kernel.GetInstance<KernelUtils.DummyLogger>().Warns[0]);
    }
}