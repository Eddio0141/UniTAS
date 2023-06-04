using MoonSharp.Interpreter;
using UniTAS.Patcher.Exceptions.Movie.Parser;

namespace Patcher.Tests.MovieRunner;

public class GlobalScopeFlagTests
{
    [Fact]
    public void GlobalScopeInvalidYield()
    {
        const string input = @"
MOVIE_CONFIG = {
    is_global_scope = true
}
-- try use yield which should not be available
adv()
";

        Assert.Throws<ScriptRuntimeException>(() => Utils.Setup(input));
    }

    [Fact]
    public void GlobalScope()
    {
        const string input = @"
MOVIE_CONFIG = {
    is_global_scope = true
}
return function()
    i = 0
    movie.frame_advance()
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
MOVIE_CONFIG = {
    is_global_scope = true
}
i = 0
i = i + 1
";

        Assert.Throws<NotReturningFunctionException>(() => Utils.Setup(input));
    }
}