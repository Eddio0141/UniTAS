using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.Movie.DefaultParsers.DefaultMoviePropertiesParser;
using UniTASPlugin.Movie.Models.Properties;

// ReSharper disable StringLiteralTypo

namespace UniTASPluginTest.MovieParsing;

public class PropertyParseUnitTests
{
    [Fact]
    public void StartFromSaveState()
    {
        var parser = new DefaultMoviePropertiesParser();
        const string input = @"name test TAS
author yuu0141
desc a test TAS
from_savestate load.whateveridk
endsave end_save";
        var expected = new PropertiesModel("test TAS", "a test TAS", "yuu0141", "end_save", "load.whateveridk");
        var actual = parser.Parse(input);

        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Author, actual.Author);
        Assert.Equal(expected.EndSavePath, actual.EndSavePath);
        Assert.Equal(expected.StartupProperties, actual.StartupProperties);
        Assert.Equal(expected.LoadSaveStatePath, actual.LoadSaveStatePath);
    }

    [Fact]
    public void StartFromReset()
    {
        var parser = new DefaultMoviePropertiesParser();
        const string input = @"name test TAS
author yuu0141
desc a test TAS
os Windows
datetime 03/28/2002
ft 0.001
resolution 900 600
unfocused
fullscreen
endsave end_save";
        var expected = new PropertiesModel("test TAS", "a test TAS", "yuu0141", "end_save",
            new StartupPropertiesModel(Os.Windows, new DateTime(2002, 3, 28), 0.001f,
                new WindowState(900, 600, true, false)));
        var actual = parser.Parse(input);

        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Author, actual.Author);
        Assert.Equal(expected.EndSavePath, actual.EndSavePath);
        Assert.Equal(expected.StartupProperties.WindowState.Height, actual.StartupProperties.WindowState.Height);
        Assert.Equal(expected.StartupProperties.WindowState.Width, actual.StartupProperties.WindowState.Width);
        Assert.Equal(expected.StartupProperties.WindowState.IsFocused, actual.StartupProperties.WindowState.IsFocused);
        Assert.Equal(expected.StartupProperties.WindowState.IsFullscreen, actual.StartupProperties.WindowState.IsFullscreen);
        Assert.Equal(expected.LoadSaveStatePath, actual.LoadSaveStatePath);
    }

    [Fact]
    public void Smallest()
    {
        var parser = new DefaultMoviePropertiesParser();
        const string input = @"from_savestate s";
        var expected = new PropertiesModel(null, null, null, null, "s");
        var actual = parser.Parse(input);

        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.Author, actual.Author);
        Assert.Equal(expected.EndSavePath, actual.EndSavePath);
        Assert.Equal(expected.StartupProperties, actual.StartupProperties);
        Assert.Equal(expected.LoadSaveStatePath, actual.LoadSaveStatePath);
    }
}