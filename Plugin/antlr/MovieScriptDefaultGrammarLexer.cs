//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from MovieScriptDefaultGrammar.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public partial class MovieScriptDefaultGrammarLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, WHITESPACE=11, COMMENT=12, COMMENT_MULTI=13, ACTIONSEPARATOR=14, 
		NEWLINE=15, SEMICOLON=16, DOLLAR=17, ASSIGN=18, PLUS_ASSIGN=19, MINUS_ASSIGN=20, 
		MULTIPLY_ASSIGN=21, DIVIDE_ASSIGN=22, MODULO_ASSIGN=23, AND=24, OR=25, 
		EQUAL=26, NOT_EQUAL=27, NOT=28, GREATER=29, LESS=30, GREATER_EQUAL=31, 
		LESS_EQUAL=32, BITWISE_AND=33, BITWISE_OR=34, BITWISE_XOR=35, BITWISE_SHIFT_LEFT=36, 
		BITWISE_SHIFT_RIGHT=37, PLUS=38, MINUS=39, MULTIPLY=40, DIVIDE=41, MODULO=42, 
		SCOPE_OPEN=43, SCOPE_CLOSE=44, ROUND_BRACKET_OPEN=45, ROUND_BRACKET_CLOSE=46, 
		INT=47, FLOAT=48, IDENTIFIER_STRING=49, STRING=50, COMMA=51;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "PIPE", "DOT", "WHITESPACE", "COMMENT", "COMMENT_MULTI", "ACTIONSEPARATOR", 
		"NEWLINE", "SEMICOLON", "DOLLAR", "ASSIGN", "PLUS_ASSIGN", "MINUS_ASSIGN", 
		"MULTIPLY_ASSIGN", "DIVIDE_ASSIGN", "MODULO_ASSIGN", "AND", "OR", "EQUAL", 
		"NOT_EQUAL", "NOT", "GREATER", "LESS", "GREATER_EQUAL", "LESS_EQUAL", 
		"BITWISE_AND", "BITWISE_OR", "BITWISE_XOR", "BITWISE_SHIFT_LEFT", "BITWISE_SHIFT_RIGHT", 
		"PLUS", "MINUS", "MULTIPLY", "DIVIDE", "MODULO", "SCOPE_OPEN", "SCOPE_CLOSE", 
		"ROUND_BRACKET_OPEN", "ROUND_BRACKET_CLOSE", "INT", "FLOAT", "IDENTIFIER_STRING", 
		"STRING", "STRING_CHAR", "ESCAPE_SEQUENCE", "COMMA"
	};


	public MovieScriptDefaultGrammarLexer(ICharStream input)
	: this(input, Console.Out, Console.Error) { }

	public MovieScriptDefaultGrammarLexer(ICharStream input, TextWriter output, TextWriter errorOutput)
	: base(input, output, errorOutput)
	{
		Interpreter = new LexerATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	private static readonly string[] _LiteralNames = {
		null, "'break'", "'continue'", "'return'", "'true'", "'false'", "'if'", 
		"'else if'", "'else'", "'fn'", "'loop'", null, null, null, null, null, 
		"';'", "'$'", "'='", "'+='", "'-='", "'*='", "'/='", "'%='", "'&&'", "'||'", 
		"'=='", "'!='", "'!'", "'>'", "'<'", "'>='", "'<='", "'&'", null, "'^'", 
		"'<<'", "'>>'", "'+'", "'-'", "'*'", "'/'", "'%'", "'{'", "'}'", "'('", 
		"')'", null, null, null, null, "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, "WHITESPACE", 
		"COMMENT", "COMMENT_MULTI", "ACTIONSEPARATOR", "NEWLINE", "SEMICOLON", 
		"DOLLAR", "ASSIGN", "PLUS_ASSIGN", "MINUS_ASSIGN", "MULTIPLY_ASSIGN", 
		"DIVIDE_ASSIGN", "MODULO_ASSIGN", "AND", "OR", "EQUAL", "NOT_EQUAL", "NOT", 
		"GREATER", "LESS", "GREATER_EQUAL", "LESS_EQUAL", "BITWISE_AND", "BITWISE_OR", 
		"BITWISE_XOR", "BITWISE_SHIFT_LEFT", "BITWISE_SHIFT_RIGHT", "PLUS", "MINUS", 
		"MULTIPLY", "DIVIDE", "MODULO", "SCOPE_OPEN", "SCOPE_CLOSE", "ROUND_BRACKET_OPEN", 
		"ROUND_BRACKET_CLOSE", "INT", "FLOAT", "IDENTIFIER_STRING", "STRING", 
		"COMMA"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "MovieScriptDefaultGrammar.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override string[] ChannelNames { get { return channelNames; } }

	public override string[] ModeNames { get { return modeNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static MovieScriptDefaultGrammarLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static int[] _serializedATN = {
		4,0,51,332,6,-1,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,2,6,7,
		6,2,7,7,7,2,8,7,8,2,9,7,9,2,10,7,10,2,11,7,11,2,12,7,12,2,13,7,13,2,14,
		7,14,2,15,7,15,2,16,7,16,2,17,7,17,2,18,7,18,2,19,7,19,2,20,7,20,2,21,
		7,21,2,22,7,22,2,23,7,23,2,24,7,24,2,25,7,25,2,26,7,26,2,27,7,27,2,28,
		7,28,2,29,7,29,2,30,7,30,2,31,7,31,2,32,7,32,2,33,7,33,2,34,7,34,2,35,
		7,35,2,36,7,36,2,37,7,37,2,38,7,38,2,39,7,39,2,40,7,40,2,41,7,41,2,42,
		7,42,2,43,7,43,2,44,7,44,2,45,7,45,2,46,7,46,2,47,7,47,2,48,7,48,2,49,
		7,49,2,50,7,50,2,51,7,51,2,52,7,52,2,53,7,53,2,54,7,54,1,0,1,0,1,0,1,0,
		1,0,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,1,2,1,2,1,2,1,2,1,2,1,
		2,1,3,1,3,1,3,1,3,1,3,1,4,1,4,1,4,1,4,1,4,1,4,1,5,1,5,1,5,1,6,1,6,1,6,
		1,6,1,6,1,6,1,6,1,6,1,7,1,7,1,7,1,7,1,7,1,8,1,8,1,8,1,9,1,9,1,9,1,9,1,
		9,1,10,1,10,1,11,1,11,1,12,4,12,174,8,12,11,12,12,12,175,1,12,1,12,1,13,
		1,13,1,13,1,13,5,13,184,8,13,10,13,12,13,187,9,13,1,13,1,13,1,14,1,14,
		1,14,1,14,5,14,195,8,14,10,14,12,14,198,9,14,1,14,1,14,1,14,1,14,1,14,
		1,15,1,15,1,16,3,16,208,8,16,1,16,1,16,1,17,1,17,1,18,1,18,1,19,1,19,1,
		20,1,20,1,20,1,21,1,21,1,21,1,22,1,22,1,22,1,23,1,23,1,23,1,24,1,24,1,
		24,1,25,1,25,1,25,1,26,1,26,1,26,1,27,1,27,1,27,1,28,1,28,1,28,1,29,1,
		29,1,30,1,30,1,31,1,31,1,32,1,32,1,32,1,33,1,33,1,33,1,34,1,34,1,35,1,
		35,1,36,1,36,1,37,1,37,1,37,1,38,1,38,1,38,1,39,1,39,1,40,1,40,1,41,1,
		41,1,42,1,42,1,43,1,43,1,44,1,44,1,45,1,45,1,46,1,46,1,47,1,47,1,48,3,
		48,288,8,48,1,48,4,48,291,8,48,11,48,12,48,292,1,49,3,49,296,8,49,1,49,
		4,49,299,8,49,11,49,12,49,300,1,49,1,49,4,49,305,8,49,11,49,12,49,306,
		1,50,1,50,5,50,311,8,50,10,50,12,50,314,9,50,1,51,1,51,1,51,5,51,319,8,
		51,10,51,12,51,322,9,51,1,51,1,51,1,52,1,52,1,53,1,53,1,53,1,54,1,54,2,
		185,196,0,55,1,1,3,2,5,3,7,4,9,5,11,6,13,7,15,8,17,9,19,10,21,0,23,0,25,
		11,27,12,29,13,31,14,33,15,35,16,37,17,39,18,41,19,43,20,45,21,47,22,49,
		23,51,24,53,25,55,26,57,27,59,28,61,29,63,30,65,31,67,32,69,33,71,34,73,
		35,75,36,77,37,79,38,81,39,83,40,85,41,87,42,89,43,91,44,93,45,95,46,97,
		47,99,48,101,49,103,50,105,0,107,0,109,51,1,0,5,2,0,9,9,32,32,1,0,48,57,
		3,0,65,90,95,95,97,122,4,0,48,57,65,90,95,95,97,122,4,0,10,10,13,13,34,
		34,92,92,339,0,1,1,0,0,0,0,3,1,0,0,0,0,5,1,0,0,0,0,7,1,0,0,0,0,9,1,0,0,
		0,0,11,1,0,0,0,0,13,1,0,0,0,0,15,1,0,0,0,0,17,1,0,0,0,0,19,1,0,0,0,0,25,
		1,0,0,0,0,27,1,0,0,0,0,29,1,0,0,0,0,31,1,0,0,0,0,33,1,0,0,0,0,35,1,0,0,
		0,0,37,1,0,0,0,0,39,1,0,0,0,0,41,1,0,0,0,0,43,1,0,0,0,0,45,1,0,0,0,0,47,
		1,0,0,0,0,49,1,0,0,0,0,51,1,0,0,0,0,53,1,0,0,0,0,55,1,0,0,0,0,57,1,0,0,
		0,0,59,1,0,0,0,0,61,1,0,0,0,0,63,1,0,0,0,0,65,1,0,0,0,0,67,1,0,0,0,0,69,
		1,0,0,0,0,71,1,0,0,0,0,73,1,0,0,0,0,75,1,0,0,0,0,77,1,0,0,0,0,79,1,0,0,
		0,0,81,1,0,0,0,0,83,1,0,0,0,0,85,1,0,0,0,0,87,1,0,0,0,0,89,1,0,0,0,0,91,
		1,0,0,0,0,93,1,0,0,0,0,95,1,0,0,0,0,97,1,0,0,0,0,99,1,0,0,0,0,101,1,0,
		0,0,0,103,1,0,0,0,0,109,1,0,0,0,1,111,1,0,0,0,3,117,1,0,0,0,5,126,1,0,
		0,0,7,133,1,0,0,0,9,138,1,0,0,0,11,144,1,0,0,0,13,147,1,0,0,0,15,155,1,
		0,0,0,17,160,1,0,0,0,19,163,1,0,0,0,21,168,1,0,0,0,23,170,1,0,0,0,25,173,
		1,0,0,0,27,179,1,0,0,0,29,190,1,0,0,0,31,204,1,0,0,0,33,207,1,0,0,0,35,
		211,1,0,0,0,37,213,1,0,0,0,39,215,1,0,0,0,41,217,1,0,0,0,43,220,1,0,0,
		0,45,223,1,0,0,0,47,226,1,0,0,0,49,229,1,0,0,0,51,232,1,0,0,0,53,235,1,
		0,0,0,55,238,1,0,0,0,57,241,1,0,0,0,59,244,1,0,0,0,61,246,1,0,0,0,63,248,
		1,0,0,0,65,250,1,0,0,0,67,253,1,0,0,0,69,256,1,0,0,0,71,258,1,0,0,0,73,
		260,1,0,0,0,75,262,1,0,0,0,77,265,1,0,0,0,79,268,1,0,0,0,81,270,1,0,0,
		0,83,272,1,0,0,0,85,274,1,0,0,0,87,276,1,0,0,0,89,278,1,0,0,0,91,280,1,
		0,0,0,93,282,1,0,0,0,95,284,1,0,0,0,97,287,1,0,0,0,99,295,1,0,0,0,101,
		308,1,0,0,0,103,315,1,0,0,0,105,325,1,0,0,0,107,327,1,0,0,0,109,330,1,
		0,0,0,111,112,5,98,0,0,112,113,5,114,0,0,113,114,5,101,0,0,114,115,5,97,
		0,0,115,116,5,107,0,0,116,2,1,0,0,0,117,118,5,99,0,0,118,119,5,111,0,0,
		119,120,5,110,0,0,120,121,5,116,0,0,121,122,5,105,0,0,122,123,5,110,0,
		0,123,124,5,117,0,0,124,125,5,101,0,0,125,4,1,0,0,0,126,127,5,114,0,0,
		127,128,5,101,0,0,128,129,5,116,0,0,129,130,5,117,0,0,130,131,5,114,0,
		0,131,132,5,110,0,0,132,6,1,0,0,0,133,134,5,116,0,0,134,135,5,114,0,0,
		135,136,5,117,0,0,136,137,5,101,0,0,137,8,1,0,0,0,138,139,5,102,0,0,139,
		140,5,97,0,0,140,141,5,108,0,0,141,142,5,115,0,0,142,143,5,101,0,0,143,
		10,1,0,0,0,144,145,5,105,0,0,145,146,5,102,0,0,146,12,1,0,0,0,147,148,
		5,101,0,0,148,149,5,108,0,0,149,150,5,115,0,0,150,151,5,101,0,0,151,152,
		5,32,0,0,152,153,5,105,0,0,153,154,5,102,0,0,154,14,1,0,0,0,155,156,5,
		101,0,0,156,157,5,108,0,0,157,158,5,115,0,0,158,159,5,101,0,0,159,16,1,
		0,0,0,160,161,5,102,0,0,161,162,5,110,0,0,162,18,1,0,0,0,163,164,5,108,
		0,0,164,165,5,111,0,0,165,166,5,111,0,0,166,167,5,112,0,0,167,20,1,0,0,
		0,168,169,5,124,0,0,169,22,1,0,0,0,170,171,5,46,0,0,171,24,1,0,0,0,172,
		174,7,0,0,0,173,172,1,0,0,0,174,175,1,0,0,0,175,173,1,0,0,0,175,176,1,
		0,0,0,176,177,1,0,0,0,177,178,6,12,0,0,178,26,1,0,0,0,179,180,5,47,0,0,
		180,181,5,47,0,0,181,185,1,0,0,0,182,184,9,0,0,0,183,182,1,0,0,0,184,187,
		1,0,0,0,185,186,1,0,0,0,185,183,1,0,0,0,186,188,1,0,0,0,187,185,1,0,0,
		0,188,189,6,13,0,0,189,28,1,0,0,0,190,191,5,47,0,0,191,192,5,42,0,0,192,
		196,1,0,0,0,193,195,9,0,0,0,194,193,1,0,0,0,195,198,1,0,0,0,196,197,1,
		0,0,0,196,194,1,0,0,0,197,199,1,0,0,0,198,196,1,0,0,0,199,200,5,42,0,0,
		200,201,5,47,0,0,201,202,1,0,0,0,202,203,6,14,0,0,203,30,1,0,0,0,204,205,
		3,21,10,0,205,32,1,0,0,0,206,208,5,13,0,0,207,206,1,0,0,0,207,208,1,0,
		0,0,208,209,1,0,0,0,209,210,5,10,0,0,210,34,1,0,0,0,211,212,5,59,0,0,212,
		36,1,0,0,0,213,214,5,36,0,0,214,38,1,0,0,0,215,216,5,61,0,0,216,40,1,0,
		0,0,217,218,5,43,0,0,218,219,5,61,0,0,219,42,1,0,0,0,220,221,5,45,0,0,
		221,222,5,61,0,0,222,44,1,0,0,0,223,224,5,42,0,0,224,225,5,61,0,0,225,
		46,1,0,0,0,226,227,5,47,0,0,227,228,5,61,0,0,228,48,1,0,0,0,229,230,5,
		37,0,0,230,231,5,61,0,0,231,50,1,0,0,0,232,233,5,38,0,0,233,234,5,38,0,
		0,234,52,1,0,0,0,235,236,5,124,0,0,236,237,5,124,0,0,237,54,1,0,0,0,238,
		239,5,61,0,0,239,240,5,61,0,0,240,56,1,0,0,0,241,242,5,33,0,0,242,243,
		5,61,0,0,243,58,1,0,0,0,244,245,5,33,0,0,245,60,1,0,0,0,246,247,5,62,0,
		0,247,62,1,0,0,0,248,249,5,60,0,0,249,64,1,0,0,0,250,251,5,62,0,0,251,
		252,5,61,0,0,252,66,1,0,0,0,253,254,5,60,0,0,254,255,5,61,0,0,255,68,1,
		0,0,0,256,257,5,38,0,0,257,70,1,0,0,0,258,259,3,21,10,0,259,72,1,0,0,0,
		260,261,5,94,0,0,261,74,1,0,0,0,262,263,5,60,0,0,263,264,5,60,0,0,264,
		76,1,0,0,0,265,266,5,62,0,0,266,267,5,62,0,0,267,78,1,0,0,0,268,269,5,
		43,0,0,269,80,1,0,0,0,270,271,5,45,0,0,271,82,1,0,0,0,272,273,5,42,0,0,
		273,84,1,0,0,0,274,275,5,47,0,0,275,86,1,0,0,0,276,277,5,37,0,0,277,88,
		1,0,0,0,278,279,5,123,0,0,279,90,1,0,0,0,280,281,5,125,0,0,281,92,1,0,
		0,0,282,283,5,40,0,0,283,94,1,0,0,0,284,285,5,41,0,0,285,96,1,0,0,0,286,
		288,5,45,0,0,287,286,1,0,0,0,287,288,1,0,0,0,288,290,1,0,0,0,289,291,7,
		1,0,0,290,289,1,0,0,0,291,292,1,0,0,0,292,290,1,0,0,0,292,293,1,0,0,0,
		293,98,1,0,0,0,294,296,5,45,0,0,295,294,1,0,0,0,295,296,1,0,0,0,296,298,
		1,0,0,0,297,299,7,1,0,0,298,297,1,0,0,0,299,300,1,0,0,0,300,298,1,0,0,
		0,300,301,1,0,0,0,301,302,1,0,0,0,302,304,3,23,11,0,303,305,7,1,0,0,304,
		303,1,0,0,0,305,306,1,0,0,0,306,304,1,0,0,0,306,307,1,0,0,0,307,100,1,
		0,0,0,308,312,7,2,0,0,309,311,7,3,0,0,310,309,1,0,0,0,311,314,1,0,0,0,
		312,310,1,0,0,0,312,313,1,0,0,0,313,102,1,0,0,0,314,312,1,0,0,0,315,320,
		5,34,0,0,316,319,3,105,52,0,317,319,3,107,53,0,318,316,1,0,0,0,318,317,
		1,0,0,0,319,322,1,0,0,0,320,318,1,0,0,0,320,321,1,0,0,0,321,323,1,0,0,
		0,322,320,1,0,0,0,323,324,5,34,0,0,324,104,1,0,0,0,325,326,8,4,0,0,326,
		106,1,0,0,0,327,328,5,92,0,0,328,329,9,0,0,0,329,108,1,0,0,0,330,331,5,
		44,0,0,331,110,1,0,0,0,13,0,175,185,196,207,287,292,295,300,306,312,318,
		320,1,6,0,0
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}