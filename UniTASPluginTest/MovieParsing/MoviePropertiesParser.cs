using FluentAssertions;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.Movie.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.MovieModels.Properties;
using UniTASPlugin.Movie.Parsers.MoviePropertyParser;

// ReSharper disable StringLiteralTypo

namespace UniTASPluginTest.MovieParsing;

public class MoviePropertiesParser
{
    [Fact]
    public void StartFromSaveState()
    {
        var parser = new MoviePropertyParser();
        const string input = @"name test TAS
author yuu0141
desc a test TAS
from_savestate load.whateveridk
endsave end_save";
        var expected = new PropertiesModel("test TAS", "a test TAS", "yuu0141", "end_save", "load.whateveridk");
        var actual = parser.Parse(input);

        expected.Should().BeEquivalentTo(actual);
    }

    [Fact]
    public void StartFromReset()
    {
        var parser = new MoviePropertyParser();
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
            new StartupPropertiesModel(Os.Windows, new(2002, 3, 28), 0.001f,
                new(900, 600, true, false)));
        var actual = parser.Parse(input);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Smallest()
    {
        var parser = new MoviePropertyParser();
        const string input = @"from_savestate s";
        var expected = new PropertiesModel(null, null, null, null, "s");
        var actual = parser.Parse(input);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void KeyAltConflict()
    {
        var parser = new MoviePropertyParser();
        const string input = @"frametime 0.001
ft 0.01";
        var parse = () => parser.Parse(input);
        parse.Should().Throw<DuplicatePropertyKeyException>();
    }

    [Fact]
    public void DupeKey()
    {
        var parser = new MoviePropertyParser();
        const string input = @"ft 0.001
ft 0.01";
        var parse = () => parser.Parse(input);
        parse.Should().Throw<DuplicatePropertyKeyException>();
    }

    [Fact]
    public void UnknownKey()
    {
        var parser = new MoviePropertyParser();
        const string input = "foo";
        var parse = () => parser.Parse(input);
        parse.Should().Throw<InvalidPropertyKeyException>();
    }

    [Fact]
    public void KeyMultiSpace()
    {
        var parser = new MoviePropertyParser();
        const string input = @"name   test TAS
            author yuu0141
 desc a test TAS
from_savestate load.whateveridk
endsave        end_save";
        var expected = new PropertiesModel("test TAS", "a test TAS", "yuu0141", "end_save", "load.whateveridk");
        var actual = parser.Parse(input);

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ConflictingKey()
    {
        var parser = new MoviePropertyParser();
        const string input = @"from_savestate test
ft 0.001";
        var parse = () => parser.Parse(input);
        parse.Should().Throw<ConflictingPropertyKeyException>();
    }

    [Fact]
    public void UnknownStartType()
    {
        var parser = new MoviePropertyParser();
        var parse = () => parser.Parse("name foo");
        parse.Should().Throw<UnknownMovieStartOptionException>();
    }
}