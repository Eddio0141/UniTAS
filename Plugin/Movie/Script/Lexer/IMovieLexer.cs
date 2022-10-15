namespace UniTASPlugin.Movie.Script.Lexer;

public interface IMovieLexer
{
    TokenBase[] TokensFromString(string input);
}