using System;
using UniTAS.Patcher.Models.Movie;

namespace Patcher.Tests.MovieRunner;

public class ConfigTests
{
    [Fact]
    public void ConfigFull()
    {
        const string input = """
                             MOVIE_CONFIG = {
                                 is_global_scope = false,
                                 frametime = 1/60,
                                 start_time = "03/28/2021 12:00:00",
                                 seed = 123,
                                 update_type = "fixedupdate",
                                 window = {
                                     width = 500,
                                     height = 600,
                                     refresh_rate = 144,
                                     fallback_res_closest = 101,
                                     resolutions = {
                                     {
                                         width = 900,
                                         height = 1000,
                                         refresh_rate = {
                                             numerator = 60,
                                             denominator = 1
                                         }
                                     },
                                     {
                                         width = 5000,
                                         height = 10000,
                                         refresh_rate = {
                                             n = 30, d = 1
                                         }
                                     },
                                     {
                                         width = 1200,
                                         height = 1400,
                                         refresh_rate = 59.5
                                     }
                                     }
                                 }
                             }
                             """;

        var properties = Utils.Setup(input).Item2;

        // 2021-03-28T12:00:00.0000000
        Assert.Equal(new(2021, 3, 28, 12, 0, 0, DateTimeKind.Utc), properties.StartupProperties.StartTime);
        Assert.Equal(1 / 60f, properties.StartupProperties.FrameTime);
        Assert.Equal(UpdateType.FixedUpdate, properties.UpdateType);
        Assert.Equal(123, properties.StartupProperties.Seed);
        var windowState = properties.StartupProperties.WindowState;
        Assert.Equal(500, windowState.CurrentResolution.Width);
        Assert.Equal(600, windowState.CurrentResolution.Height);
        Assert.Equal(144, windowState.CurrentResolution.RefreshRateWrap.Rate);
        Assert.Equal(3, windowState.ExtraResolutions.Length);
        Assert.Equal(101, windowState.ExtraResolutionClosest);

        var res = windowState.ExtraResolutions[0];
        Assert.Equal(900, res.Width);
        Assert.Equal(1000, res.Height);
        Assert.Equal(60u, res.RefreshRateWrap.Numerator);
        Assert.Equal(1u, res.RefreshRateWrap.Denominator);

        res = windowState.ExtraResolutions[1];
        Assert.Equal(5000, res.Width);
        Assert.Equal(10000, res.Height);
        Assert.Equal(30u, res.RefreshRateWrap.Numerator);
        Assert.Equal(1u, res.RefreshRateWrap.Denominator);

        res = windowState.ExtraResolutions[2];
        Assert.Equal(1200, res.Width);
        Assert.Equal(1400, res.Height);
        Assert.Equal(59.5, res.RefreshRateWrap.Rate);
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