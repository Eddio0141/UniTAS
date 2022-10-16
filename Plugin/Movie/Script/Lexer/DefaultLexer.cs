using System.Collections.Generic;
using UniTASPlugin.Movie.Script.LexerTokens;

namespace UniTASPlugin.Movie.Script.Lexer;

public class DefaultLexer : IMovieLexer
{
    public TokenBase[] TokensFromString(string input)
    {
        var tokens = new List<TokenBase>();
        
        while (input.Length > 0)
        {
            var nextTokenResult = NextToken(input);
            input = nextTokenResult.Key;
            tokens.Add(nextTokenResult.Value);
        }

        return tokens.ToArray();
    }

    private KeyValuePair<string, TokenBase> NextToken(string input)
    {
        input = input.TrimStart();
        
        if (input.StartsWith("$"))
    }
}