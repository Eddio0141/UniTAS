using MoonSharp.Interpreter;
using UniTAS.Plugin.Movie.Parsers.Exceptions;

namespace UniTAS.Plugin.Tests.MovieRunner;

public class GlobalScopeFlagTests
{
    [Fact]
    public void GlobalScopeInvalidYield()
    {
        const string input = @"
GLOBAL_SCOPE = true
-- try use yield which should not be available
adv()
";

        Assert.Throws<ScriptRuntimeException>(() => Utils.Setup(input));
    }

    [Fact]
    public void GlobalScope()
    {
        const string input = @"
GLOBAL_SCOPE = true
return function()
    i = 0
    adv()
    i = i + 1
end
";

        var movieRunner = Utils.Setup(input).Item1;

        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.False(movieRunner.Finished);
        movieRunner.Update();
        Assert.True(movieRunner.Finished);
    }

    [Fact]
    public void GlobalScopeInvalidNoReturn()
    {
        // don't return a function
        const string input = @"
GLOBAL_SCOPE = true
i = 0
i = i + 1
";

        Assert.Throws<NotReturningFunctionException>(() => Utils.Setup(input));
    }
}