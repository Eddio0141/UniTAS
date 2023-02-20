using Antlr4.Runtime.Misc;
using BepInEx.Logging;
using FluentAssertions;
using UniTAS.Plugin.Logger;
using UniTAS.Plugin.Movie.EngineMethods;
using UniTAS.Plugin.Movie.MovieModels.Script;
using UniTAS.Plugin.Movie.Parsers.MovieScriptParser;

namespace UniTAS.Plugin.Tests.MovieParsing;

public class MovieParseFail
{
    private class FakeLogger : IMovieLogger
    {
        public void LogError(object data)
        {
        }

        public void LogInfo(object data)
        {
        }

#pragma warning disable 67
        public event EventHandler<LogEventArgs>? OnLog;
#pragma warning restore 67
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