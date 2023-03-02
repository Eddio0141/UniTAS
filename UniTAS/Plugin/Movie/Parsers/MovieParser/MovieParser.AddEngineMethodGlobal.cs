using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public partial class MovieParser
{
    private static void AddEngineMethodGlobal(IMovieEngine engine)
    {
        var script = engine.Script;

        // alias method to coroutine.yield as adv
        const string advOverload = @"
function adv(frames)
    if type(frames) ~= 'number' then
        frames = 1
    end
    for i = 1, frames do
        coroutine.yield()
    end
end";

        script.DoString(advOverload);
    }
}