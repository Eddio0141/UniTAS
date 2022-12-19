using Antlr4.Runtime.Misc;
using FluentAssertions;
using UniTASPlugin.Movie.MovieRunner.EngineMethods;
using UniTASPlugin.Movie.MovieRunner.MovieModels.Script;
using UniTASPlugin.Movie.MovieRunner.Parsers.MovieScriptParser;

namespace UniTASPluginTest.MovieParsing;

public class DefaultMovieParseFail
{
    private static ScriptModel Setup(string input)
    {
        var parser = new MovieScriptParser(new[] { new PrintExternalMethod() });
        var methods = parser.Parse(input).ToList();
        var mainMethod = methods.First(x => x.Name == null);
        var definedMethods = methods.Where(x => x.Name != null);
        return new ScriptModel(mainMethod, definedMethods);
    }

    [Fact]
    public void ParseError()
    {
        var parser = () => Setup(@"$value = 10
fn method(arg) { }
method(no)");

        parser.Should().ThrowExactly<ParseCanceledException>();
    }
}