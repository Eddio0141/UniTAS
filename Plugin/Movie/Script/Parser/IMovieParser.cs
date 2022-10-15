using UniTASPlugin.Movie.Script.Lexer;
using UniTASPlugin.Movie.Script.LowLevel.OpCodes;

namespace UniTASPlugin.Movie.Script.Parser;

public interface IMovieParser
{
    OpCodeBase[] ParseTokens(TokenBase[] tokens);
}