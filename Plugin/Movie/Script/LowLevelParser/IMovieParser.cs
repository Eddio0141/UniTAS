using UniTASPlugin.Movie.Script.LexerTokens;
using UniTASPlugin.Movie.Script.LowLevel.OpCodes;

namespace UniTASPlugin.Movie.Script.LowLevelParser;

public interface IMovieParser
{
    OpCodeBase[] ParseTokens(TokenBase[] tokens);
}