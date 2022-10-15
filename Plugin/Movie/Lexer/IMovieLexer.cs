namespace UniTASPlugin.Movie.Lexer;

public interface IMovieLexer
{
    TokenBase[] TokensFromString(string input);
}