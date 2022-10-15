using UniTASPlugin.Movie.Lexer;
using UniTASPlugin.Movie.Model.Script.LowLevel.OpCodes;

namespace UniTASPlugin.Movie.Parser;

public interface IMovieParser
{
    OpCodeBase[] ParseTokens(TokenBase[] tokens);
}