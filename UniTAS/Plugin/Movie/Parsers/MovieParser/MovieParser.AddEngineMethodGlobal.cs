using UniTAS.Plugin.Movie.Engine;

namespace UniTAS.Plugin.Movie.Parsers.MovieParser;

public partial class MovieParser
{
    /// <summary>
    /// Directly adds to the lua script
    /// </summary>
    /// <param name="engine">The movie engine</param>
    private static void AddEngineMethodRaw(IMovieEngine engine)
    {
        var script = engine.Script;

        // alias method to coroutine.yield as frame_advance
        const string advOverload = @"
function frame_advance(frames)
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