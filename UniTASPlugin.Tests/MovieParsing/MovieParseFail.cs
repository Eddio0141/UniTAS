using Antlr4.Runtime.Misc;
using FluentAssertions;
using UniTASPlugin.Logger;
using UniTASPlugin.Movie.EngineMethods;
using UniTASPlugin.Movie.MovieModels.Script;
using UniTASPlugin.Movie.Parsers.MovieScriptParser;

namespace UniTASPlugin.Tests.MovieParsing;

public class MovieParseFail
{
    private class FakeLogger : ILogger
    {
        public void LogFatal(object data)
        {
        }

        public void LogError(object data)
        {
        }

        public void LogWarning(object data)
        {
        }

        public void LogMessage(object data)
        {
        }

        public void LogInfo(object data)
        {
        }

        public void LogDebug(object data)
        {
        }
    }

    private static ScriptModel Setup(string input)
    {
        var parser = new MovieScriptParser(new[] { new PrintExternalMethod(new FakeLogger()) });
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