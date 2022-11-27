//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.9.3
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from MovieScriptDefaultGrammar.g4 by ANTLR 4.9.3

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

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.9.3")]
[System.CLSCompliant(false)]
public partial class MovieScriptDefaultGrammarLexer : Lexer {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, WHITESPACE=11, COMMENT=12, COMMENT_MULTI=13, ACTIONSEPARATOR=14, 
		NEWLINE=15, SEMICOLON=16, TUPLE_DECONSTRUCTOR_START=17, DOLLAR=18, ASSIGN=19, 
		PLUS_ASSIGN=20, MINUS_ASSIGN=21, MULTIPLY_ASSIGN=22, DIVIDE_ASSIGN=23, 
		MODULO_ASSIGN=24, AND=25, OR=26, EQUAL=27, NOT_EQUAL=28, NOT=29, GREATER=30, 
		LESS=31, GREATER_EQUAL=32, LESS_EQUAL=33, BITWISE_AND=34, BITWISE_OR=35, 
		BITWISE_XOR=36, BITWISE_SHIFT_LEFT=37, BITWISE_SHIFT_RIGHT=38, PLUS=39, 
		MINUS=40, MULTIPLY=41, DIVIDE=42, MODULO=43, SCOPE_OPEN=44, SCOPE_CLOSE=45, 
		ROUND_BRACKET_OPEN=46, ROUND_BRACKET_CLOSE=47, INT=48, FLOAT=49, IGNORE_VARIABLE_NAME=50, 
		IDENTIFIER_STRING=51, STRING=52, COMMA=53, ANY=54;
	public static string[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static string[] modeNames = {
		"DEFAULT_MODE"
	};

	public static readonly string[] ruleNames = {
		"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
		"T__9", "PIPE", "DOT", "WHITESPACE", "COMMENT", "COMMENT_MULTI", "ACTIONSEPARATOR", 
		"NEWLINE", "SEMICOLON", "TUPLE_DECONSTRUCTOR_START", "DOLLAR", "ASSIGN", 
		"PLUS_ASSIGN", "MINUS_ASSIGN", "MULTIPLY_ASSIGN", "DIVIDE_ASSIGN", "MODULO_ASSIGN", 
		"AND", "OR", "EQUAL", "NOT_EQUAL", "NOT", "GREATER", "LESS", "GREATER_EQUAL", 
		"LESS_EQUAL", "BITWISE_AND", "BITWISE_OR", "BITWISE_XOR", "BITWISE_SHIFT_LEFT", 
		"BITWISE_SHIFT_RIGHT", "PLUS", "MINUS", "MULTIPLY", "DIVIDE", "MODULO", 
		"SCOPE_OPEN", "SCOPE_CLOSE", "ROUND_BRACKET_OPEN", "ROUND_BRACKET_CLOSE", 
		"INT", "FLOAT", "IGNORE_VARIABLE_NAME", "IDENTIFIER_STRING", "STRING", 
		"STRING_CHAR", "ESCAPE_SEQUENCE", "COMMA", "ANY"
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
		"';'", null, "'$'", "'='", "'+='", "'-='", "'*='", "'/='", "'%='", "'&&'", 
		"'||'", "'=='", "'!='", "'!'", "'>'", "'<'", "'>='", "'<='", "'&'", null, 
		"'^'", "'<<'", "'>>'", "'+'", "'-'", "'*'", "'/'", "'%'", "'{'", "'}'", 
		"'('", "')'", null, null, "'_'", null, null, "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, "WHITESPACE", 
		"COMMENT", "COMMENT_MULTI", "ACTIONSEPARATOR", "NEWLINE", "SEMICOLON", 
		"TUPLE_DECONSTRUCTOR_START", "DOLLAR", "ASSIGN", "PLUS_ASSIGN", "MINUS_ASSIGN", 
		"MULTIPLY_ASSIGN", "DIVIDE_ASSIGN", "MODULO_ASSIGN", "AND", "OR", "EQUAL", 
		"NOT_EQUAL", "NOT", "GREATER", "LESS", "GREATER_EQUAL", "LESS_EQUAL", 
		"BITWISE_AND", "BITWISE_OR", "BITWISE_XOR", "BITWISE_SHIFT_LEFT", "BITWISE_SHIFT_RIGHT", 
		"PLUS", "MINUS", "MULTIPLY", "DIVIDE", "MODULO", "SCOPE_OPEN", "SCOPE_CLOSE", 
		"ROUND_BRACKET_OPEN", "ROUND_BRACKET_CLOSE", "INT", "FLOAT", "IGNORE_VARIABLE_NAME", 
		"IDENTIFIER_STRING", "STRING", "COMMA", "ANY"
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

	public override string SerializedAtn { get { return new string(_serializedATN); } }

	static MovieScriptDefaultGrammarLexer() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}
	private static char[] _serializedATN = {
		'\x3', '\x608B', '\xA72A', '\x8133', '\xB9ED', '\x417C', '\x3BE7', '\x7786', 
		'\x5964', '\x2', '\x38', '\x15B', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
		'\x4', '\x3', '\t', '\x3', '\x4', '\x4', '\t', '\x4', '\x4', '\x5', '\t', 
		'\x5', '\x4', '\x6', '\t', '\x6', '\x4', '\a', '\t', '\a', '\x4', '\b', 
		'\t', '\b', '\x4', '\t', '\t', '\t', '\x4', '\n', '\t', '\n', '\x4', '\v', 
		'\t', '\v', '\x4', '\f', '\t', '\f', '\x4', '\r', '\t', '\r', '\x4', '\xE', 
		'\t', '\xE', '\x4', '\xF', '\t', '\xF', '\x4', '\x10', '\t', '\x10', '\x4', 
		'\x11', '\t', '\x11', '\x4', '\x12', '\t', '\x12', '\x4', '\x13', '\t', 
		'\x13', '\x4', '\x14', '\t', '\x14', '\x4', '\x15', '\t', '\x15', '\x4', 
		'\x16', '\t', '\x16', '\x4', '\x17', '\t', '\x17', '\x4', '\x18', '\t', 
		'\x18', '\x4', '\x19', '\t', '\x19', '\x4', '\x1A', '\t', '\x1A', '\x4', 
		'\x1B', '\t', '\x1B', '\x4', '\x1C', '\t', '\x1C', '\x4', '\x1D', '\t', 
		'\x1D', '\x4', '\x1E', '\t', '\x1E', '\x4', '\x1F', '\t', '\x1F', '\x4', 
		' ', '\t', ' ', '\x4', '!', '\t', '!', '\x4', '\"', '\t', '\"', '\x4', 
		'#', '\t', '#', '\x4', '$', '\t', '$', '\x4', '%', '\t', '%', '\x4', '&', 
		'\t', '&', '\x4', '\'', '\t', '\'', '\x4', '(', '\t', '(', '\x4', ')', 
		'\t', ')', '\x4', '*', '\t', '*', '\x4', '+', '\t', '+', '\x4', ',', '\t', 
		',', '\x4', '-', '\t', '-', '\x4', '.', '\t', '.', '\x4', '/', '\t', '/', 
		'\x4', '\x30', '\t', '\x30', '\x4', '\x31', '\t', '\x31', '\x4', '\x32', 
		'\t', '\x32', '\x4', '\x33', '\t', '\x33', '\x4', '\x34', '\t', '\x34', 
		'\x4', '\x35', '\t', '\x35', '\x4', '\x36', '\t', '\x36', '\x4', '\x37', 
		'\t', '\x37', '\x4', '\x38', '\t', '\x38', '\x4', '\x39', '\t', '\x39', 
		'\x4', ':', '\t', ':', '\x4', ';', '\t', ';', '\x3', '\x2', '\x3', '\x2', 
		'\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', 
		'\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', 
		'\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', 
		'\x3', '\x6', '\x3', '\a', '\x3', '\a', '\x3', '\a', '\x3', '\b', '\x3', 
		'\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', 
		'\x3', '\b', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', 
		'\t', '\x3', '\n', '\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', 
		'\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', '\f', '\x3', '\f', '\x3', 
		'\r', '\x3', '\r', '\x3', '\xE', '\x6', '\xE', '\xB6', '\n', '\xE', '\r', 
		'\xE', '\xE', '\xE', '\xB7', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', 
		'\x3', '\xF', '\x3', '\xF', '\x3', '\xF', '\a', '\xF', '\xC0', '\n', '\xF', 
		'\f', '\xF', '\xE', '\xF', '\xC3', '\v', '\xF', '\x3', '\xF', '\x3', '\xF', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\a', '\x10', 
		'\xCB', '\n', '\x10', '\f', '\x10', '\xE', '\x10', '\xCE', '\v', '\x10', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', 
		'\x3', '\x11', '\x3', '\x11', '\x3', '\x12', '\x5', '\x12', '\xD8', '\n', 
		'\x12', '\x3', '\x12', '\x3', '\x12', '\x3', '\x13', '\x3', '\x13', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x14', '\x3', '\x15', '\x3', '\x15', '\x3', 
		'\x16', '\x3', '\x16', '\x3', '\x17', '\x3', '\x17', '\x3', '\x17', '\x3', 
		'\x18', '\x3', '\x18', '\x3', '\x18', '\x3', '\x19', '\x3', '\x19', '\x3', 
		'\x19', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1B', '\x3', 
		'\x1B', '\x3', '\x1B', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1C', '\x3', 
		'\x1D', '\x3', '\x1D', '\x3', '\x1D', '\x3', '\x1E', '\x3', '\x1E', '\x3', 
		'\x1E', '\x3', '\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', ' ', '\x3', 
		' ', '\x3', '!', '\x3', '!', '\x3', '\"', '\x3', '\"', '\x3', '#', '\x3', 
		'#', '\x3', '#', '\x3', '$', '\x3', '$', '\x3', '$', '\x3', '%', '\x3', 
		'%', '\x3', '&', '\x3', '&', '\x3', '\'', '\x3', '\'', '\x3', '(', '\x3', 
		'(', '\x3', '(', '\x3', ')', '\x3', ')', '\x3', ')', '\x3', '*', '\x3', 
		'*', '\x3', '+', '\x3', '+', '\x3', ',', '\x3', ',', '\x3', '-', '\x3', 
		'-', '\x3', '.', '\x3', '.', '\x3', '/', '\x3', '/', '\x3', '\x30', '\x3', 
		'\x30', '\x3', '\x31', '\x3', '\x31', '\x3', '\x32', '\x3', '\x32', '\x3', 
		'\x33', '\x5', '\x33', '\x12B', '\n', '\x33', '\x3', '\x33', '\x6', '\x33', 
		'\x12E', '\n', '\x33', '\r', '\x33', '\xE', '\x33', '\x12F', '\x3', '\x34', 
		'\x5', '\x34', '\x133', '\n', '\x34', '\x3', '\x34', '\x6', '\x34', '\x136', 
		'\n', '\x34', '\r', '\x34', '\xE', '\x34', '\x137', '\x3', '\x34', '\x3', 
		'\x34', '\x6', '\x34', '\x13C', '\n', '\x34', '\r', '\x34', '\xE', '\x34', 
		'\x13D', '\x3', '\x35', '\x3', '\x35', '\x3', '\x36', '\x3', '\x36', '\a', 
		'\x36', '\x144', '\n', '\x36', '\f', '\x36', '\xE', '\x36', '\x147', '\v', 
		'\x36', '\x3', '\x37', '\x3', '\x37', '\x3', '\x37', '\a', '\x37', '\x14C', 
		'\n', '\x37', '\f', '\x37', '\xE', '\x37', '\x14F', '\v', '\x37', '\x3', 
		'\x37', '\x3', '\x37', '\x3', '\x38', '\x3', '\x38', '\x3', '\x39', '\x3', 
		'\x39', '\x3', '\x39', '\x3', ':', '\x3', ':', '\x3', ';', '\x3', ';', 
		'\x4', '\xC1', '\xCC', '\x2', '<', '\x3', '\x3', '\x5', '\x4', '\a', '\x5', 
		'\t', '\x6', '\v', '\a', '\r', '\b', '\xF', '\t', '\x11', '\n', '\x13', 
		'\v', '\x15', '\f', '\x17', '\x2', '\x19', '\x2', '\x1B', '\r', '\x1D', 
		'\xE', '\x1F', '\xF', '!', '\x10', '#', '\x11', '%', '\x12', '\'', '\x13', 
		')', '\x14', '+', '\x15', '-', '\x16', '/', '\x17', '\x31', '\x18', '\x33', 
		'\x19', '\x35', '\x1A', '\x37', '\x1B', '\x39', '\x1C', ';', '\x1D', '=', 
		'\x1E', '?', '\x1F', '\x41', ' ', '\x43', '!', '\x45', '\"', 'G', '#', 
		'I', '$', 'K', '%', 'M', '&', 'O', '\'', 'Q', '(', 'S', ')', 'U', '*', 
		'W', '+', 'Y', ',', '[', '-', ']', '.', '_', '/', '\x61', '\x30', '\x63', 
		'\x31', '\x65', '\x32', 'g', '\x33', 'i', '\x34', 'k', '\x35', 'm', '\x36', 
		'o', '\x2', 'q', '\x2', 's', '\x37', 'u', '\x38', '\x3', '\x2', '\a', 
		'\x4', '\x2', '\v', '\v', '\"', '\"', '\x3', '\x2', '\x32', ';', '\x5', 
		'\x2', '\x43', '\\', '\x61', '\x61', '\x63', '|', '\x6', '\x2', '\x32', 
		';', '\x43', '\\', '\x61', '\x61', '\x63', '|', '\x6', '\x2', '\f', '\f', 
		'\xF', '\xF', '$', '$', '^', '^', '\x2', '\x162', '\x2', '\x3', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x5', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\a', '\x3', '\x2', '\x2', '\x2', '\x2', '\t', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\v', '\x3', '\x2', '\x2', '\x2', '\x2', '\r', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\xF', '\x3', '\x2', '\x2', '\x2', '\x2', '\x11', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x13', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x15', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1B', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x1D', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1F', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '!', '\x3', '\x2', '\x2', '\x2', '\x2', '#', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '%', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\'', '\x3', '\x2', '\x2', '\x2', '\x2', ')', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '+', '\x3', '\x2', '\x2', '\x2', '\x2', '-', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '/', '\x3', '\x2', '\x2', '\x2', '\x2', '\x31', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x33', '\x3', '\x2', '\x2', '\x2', '\x2', '\x35', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x37', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x39', '\x3', '\x2', '\x2', '\x2', '\x2', ';', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '=', '\x3', '\x2', '\x2', '\x2', '\x2', '?', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x41', '\x3', '\x2', '\x2', '\x2', '\x2', '\x43', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x45', '\x3', '\x2', '\x2', '\x2', 
		'\x2', 'G', '\x3', '\x2', '\x2', '\x2', '\x2', 'I', '\x3', '\x2', '\x2', 
		'\x2', '\x2', 'K', '\x3', '\x2', '\x2', '\x2', '\x2', 'M', '\x3', '\x2', 
		'\x2', '\x2', '\x2', 'O', '\x3', '\x2', '\x2', '\x2', '\x2', 'Q', '\x3', 
		'\x2', '\x2', '\x2', '\x2', 'S', '\x3', '\x2', '\x2', '\x2', '\x2', 'U', 
		'\x3', '\x2', '\x2', '\x2', '\x2', 'W', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'Y', '\x3', '\x2', '\x2', '\x2', '\x2', '[', '\x3', '\x2', '\x2', '\x2', 
		'\x2', ']', '\x3', '\x2', '\x2', '\x2', '\x2', '_', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x61', '\x3', '\x2', '\x2', '\x2', '\x2', '\x63', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x65', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'g', '\x3', '\x2', '\x2', '\x2', '\x2', 'i', '\x3', '\x2', '\x2', '\x2', 
		'\x2', 'k', '\x3', '\x2', '\x2', '\x2', '\x2', 'm', '\x3', '\x2', '\x2', 
		'\x2', '\x2', 's', '\x3', '\x2', '\x2', '\x2', '\x2', 'u', '\x3', '\x2', 
		'\x2', '\x2', '\x3', 'w', '\x3', '\x2', '\x2', '\x2', '\x5', '}', '\x3', 
		'\x2', '\x2', '\x2', '\a', '\x86', '\x3', '\x2', '\x2', '\x2', '\t', '\x8D', 
		'\x3', '\x2', '\x2', '\x2', '\v', '\x92', '\x3', '\x2', '\x2', '\x2', 
		'\r', '\x98', '\x3', '\x2', '\x2', '\x2', '\xF', '\x9B', '\x3', '\x2', 
		'\x2', '\x2', '\x11', '\xA3', '\x3', '\x2', '\x2', '\x2', '\x13', '\xA8', 
		'\x3', '\x2', '\x2', '\x2', '\x15', '\xAB', '\x3', '\x2', '\x2', '\x2', 
		'\x17', '\xB0', '\x3', '\x2', '\x2', '\x2', '\x19', '\xB2', '\x3', '\x2', 
		'\x2', '\x2', '\x1B', '\xB5', '\x3', '\x2', '\x2', '\x2', '\x1D', '\xBB', 
		'\x3', '\x2', '\x2', '\x2', '\x1F', '\xC6', '\x3', '\x2', '\x2', '\x2', 
		'!', '\xD4', '\x3', '\x2', '\x2', '\x2', '#', '\xD7', '\x3', '\x2', '\x2', 
		'\x2', '%', '\xDB', '\x3', '\x2', '\x2', '\x2', '\'', '\xDD', '\x3', '\x2', 
		'\x2', '\x2', ')', '\xE0', '\x3', '\x2', '\x2', '\x2', '+', '\xE2', '\x3', 
		'\x2', '\x2', '\x2', '-', '\xE4', '\x3', '\x2', '\x2', '\x2', '/', '\xE7', 
		'\x3', '\x2', '\x2', '\x2', '\x31', '\xEA', '\x3', '\x2', '\x2', '\x2', 
		'\x33', '\xED', '\x3', '\x2', '\x2', '\x2', '\x35', '\xF0', '\x3', '\x2', 
		'\x2', '\x2', '\x37', '\xF3', '\x3', '\x2', '\x2', '\x2', '\x39', '\xF6', 
		'\x3', '\x2', '\x2', '\x2', ';', '\xF9', '\x3', '\x2', '\x2', '\x2', '=', 
		'\xFC', '\x3', '\x2', '\x2', '\x2', '?', '\xFF', '\x3', '\x2', '\x2', 
		'\x2', '\x41', '\x101', '\x3', '\x2', '\x2', '\x2', '\x43', '\x103', '\x3', 
		'\x2', '\x2', '\x2', '\x45', '\x105', '\x3', '\x2', '\x2', '\x2', 'G', 
		'\x108', '\x3', '\x2', '\x2', '\x2', 'I', '\x10B', '\x3', '\x2', '\x2', 
		'\x2', 'K', '\x10D', '\x3', '\x2', '\x2', '\x2', 'M', '\x10F', '\x3', 
		'\x2', '\x2', '\x2', 'O', '\x111', '\x3', '\x2', '\x2', '\x2', 'Q', '\x114', 
		'\x3', '\x2', '\x2', '\x2', 'S', '\x117', '\x3', '\x2', '\x2', '\x2', 
		'U', '\x119', '\x3', '\x2', '\x2', '\x2', 'W', '\x11B', '\x3', '\x2', 
		'\x2', '\x2', 'Y', '\x11D', '\x3', '\x2', '\x2', '\x2', '[', '\x11F', 
		'\x3', '\x2', '\x2', '\x2', ']', '\x121', '\x3', '\x2', '\x2', '\x2', 
		'_', '\x123', '\x3', '\x2', '\x2', '\x2', '\x61', '\x125', '\x3', '\x2', 
		'\x2', '\x2', '\x63', '\x127', '\x3', '\x2', '\x2', '\x2', '\x65', '\x12A', 
		'\x3', '\x2', '\x2', '\x2', 'g', '\x132', '\x3', '\x2', '\x2', '\x2', 
		'i', '\x13F', '\x3', '\x2', '\x2', '\x2', 'k', '\x141', '\x3', '\x2', 
		'\x2', '\x2', 'm', '\x148', '\x3', '\x2', '\x2', '\x2', 'o', '\x152', 
		'\x3', '\x2', '\x2', '\x2', 'q', '\x154', '\x3', '\x2', '\x2', '\x2', 
		's', '\x157', '\x3', '\x2', '\x2', '\x2', 'u', '\x159', '\x3', '\x2', 
		'\x2', '\x2', 'w', 'x', '\a', '\x64', '\x2', '\x2', 'x', 'y', '\a', 't', 
		'\x2', '\x2', 'y', 'z', '\a', 'g', '\x2', '\x2', 'z', '{', '\a', '\x63', 
		'\x2', '\x2', '{', '|', '\a', 'm', '\x2', '\x2', '|', '\x4', '\x3', '\x2', 
		'\x2', '\x2', '}', '~', '\a', '\x65', '\x2', '\x2', '~', '\x7F', '\a', 
		'q', '\x2', '\x2', '\x7F', '\x80', '\a', 'p', '\x2', '\x2', '\x80', '\x81', 
		'\a', 'v', '\x2', '\x2', '\x81', '\x82', '\a', 'k', '\x2', '\x2', '\x82', 
		'\x83', '\a', 'p', '\x2', '\x2', '\x83', '\x84', '\a', 'w', '\x2', '\x2', 
		'\x84', '\x85', '\a', 'g', '\x2', '\x2', '\x85', '\x6', '\x3', '\x2', 
		'\x2', '\x2', '\x86', '\x87', '\a', 't', '\x2', '\x2', '\x87', '\x88', 
		'\a', 'g', '\x2', '\x2', '\x88', '\x89', '\a', 'v', '\x2', '\x2', '\x89', 
		'\x8A', '\a', 'w', '\x2', '\x2', '\x8A', '\x8B', '\a', 't', '\x2', '\x2', 
		'\x8B', '\x8C', '\a', 'p', '\x2', '\x2', '\x8C', '\b', '\x3', '\x2', '\x2', 
		'\x2', '\x8D', '\x8E', '\a', 'v', '\x2', '\x2', '\x8E', '\x8F', '\a', 
		't', '\x2', '\x2', '\x8F', '\x90', '\a', 'w', '\x2', '\x2', '\x90', '\x91', 
		'\a', 'g', '\x2', '\x2', '\x91', '\n', '\x3', '\x2', '\x2', '\x2', '\x92', 
		'\x93', '\a', 'h', '\x2', '\x2', '\x93', '\x94', '\a', '\x63', '\x2', 
		'\x2', '\x94', '\x95', '\a', 'n', '\x2', '\x2', '\x95', '\x96', '\a', 
		'u', '\x2', '\x2', '\x96', '\x97', '\a', 'g', '\x2', '\x2', '\x97', '\f', 
		'\x3', '\x2', '\x2', '\x2', '\x98', '\x99', '\a', 'k', '\x2', '\x2', '\x99', 
		'\x9A', '\a', 'h', '\x2', '\x2', '\x9A', '\xE', '\x3', '\x2', '\x2', '\x2', 
		'\x9B', '\x9C', '\a', 'g', '\x2', '\x2', '\x9C', '\x9D', '\a', 'n', '\x2', 
		'\x2', '\x9D', '\x9E', '\a', 'u', '\x2', '\x2', '\x9E', '\x9F', '\a', 
		'g', '\x2', '\x2', '\x9F', '\xA0', '\a', '\"', '\x2', '\x2', '\xA0', '\xA1', 
		'\a', 'k', '\x2', '\x2', '\xA1', '\xA2', '\a', 'h', '\x2', '\x2', '\xA2', 
		'\x10', '\x3', '\x2', '\x2', '\x2', '\xA3', '\xA4', '\a', 'g', '\x2', 
		'\x2', '\xA4', '\xA5', '\a', 'n', '\x2', '\x2', '\xA5', '\xA6', '\a', 
		'u', '\x2', '\x2', '\xA6', '\xA7', '\a', 'g', '\x2', '\x2', '\xA7', '\x12', 
		'\x3', '\x2', '\x2', '\x2', '\xA8', '\xA9', '\a', 'h', '\x2', '\x2', '\xA9', 
		'\xAA', '\a', 'p', '\x2', '\x2', '\xAA', '\x14', '\x3', '\x2', '\x2', 
		'\x2', '\xAB', '\xAC', '\a', 'n', '\x2', '\x2', '\xAC', '\xAD', '\a', 
		'q', '\x2', '\x2', '\xAD', '\xAE', '\a', 'q', '\x2', '\x2', '\xAE', '\xAF', 
		'\a', 'r', '\x2', '\x2', '\xAF', '\x16', '\x3', '\x2', '\x2', '\x2', '\xB0', 
		'\xB1', '\a', '~', '\x2', '\x2', '\xB1', '\x18', '\x3', '\x2', '\x2', 
		'\x2', '\xB2', '\xB3', '\a', '\x30', '\x2', '\x2', '\xB3', '\x1A', '\x3', 
		'\x2', '\x2', '\x2', '\xB4', '\xB6', '\t', '\x2', '\x2', '\x2', '\xB5', 
		'\xB4', '\x3', '\x2', '\x2', '\x2', '\xB6', '\xB7', '\x3', '\x2', '\x2', 
		'\x2', '\xB7', '\xB5', '\x3', '\x2', '\x2', '\x2', '\xB7', '\xB8', '\x3', 
		'\x2', '\x2', '\x2', '\xB8', '\xB9', '\x3', '\x2', '\x2', '\x2', '\xB9', 
		'\xBA', '\b', '\xE', '\x2', '\x2', '\xBA', '\x1C', '\x3', '\x2', '\x2', 
		'\x2', '\xBB', '\xBC', '\a', '\x31', '\x2', '\x2', '\xBC', '\xBD', '\a', 
		'\x31', '\x2', '\x2', '\xBD', '\xC1', '\x3', '\x2', '\x2', '\x2', '\xBE', 
		'\xC0', '\v', '\x2', '\x2', '\x2', '\xBF', '\xBE', '\x3', '\x2', '\x2', 
		'\x2', '\xC0', '\xC3', '\x3', '\x2', '\x2', '\x2', '\xC1', '\xC2', '\x3', 
		'\x2', '\x2', '\x2', '\xC1', '\xBF', '\x3', '\x2', '\x2', '\x2', '\xC2', 
		'\xC4', '\x3', '\x2', '\x2', '\x2', '\xC3', '\xC1', '\x3', '\x2', '\x2', 
		'\x2', '\xC4', '\xC5', '\b', '\xF', '\x2', '\x2', '\xC5', '\x1E', '\x3', 
		'\x2', '\x2', '\x2', '\xC6', '\xC7', '\a', '\x31', '\x2', '\x2', '\xC7', 
		'\xC8', '\a', ',', '\x2', '\x2', '\xC8', '\xCC', '\x3', '\x2', '\x2', 
		'\x2', '\xC9', '\xCB', '\v', '\x2', '\x2', '\x2', '\xCA', '\xC9', '\x3', 
		'\x2', '\x2', '\x2', '\xCB', '\xCE', '\x3', '\x2', '\x2', '\x2', '\xCC', 
		'\xCD', '\x3', '\x2', '\x2', '\x2', '\xCC', '\xCA', '\x3', '\x2', '\x2', 
		'\x2', '\xCD', '\xCF', '\x3', '\x2', '\x2', '\x2', '\xCE', '\xCC', '\x3', 
		'\x2', '\x2', '\x2', '\xCF', '\xD0', '\a', ',', '\x2', '\x2', '\xD0', 
		'\xD1', '\a', '\x31', '\x2', '\x2', '\xD1', '\xD2', '\x3', '\x2', '\x2', 
		'\x2', '\xD2', '\xD3', '\b', '\x10', '\x2', '\x2', '\xD3', ' ', '\x3', 
		'\x2', '\x2', '\x2', '\xD4', '\xD5', '\x5', '\x17', '\f', '\x2', '\xD5', 
		'\"', '\x3', '\x2', '\x2', '\x2', '\xD6', '\xD8', '\a', '\xF', '\x2', 
		'\x2', '\xD7', '\xD6', '\x3', '\x2', '\x2', '\x2', '\xD7', '\xD8', '\x3', 
		'\x2', '\x2', '\x2', '\xD8', '\xD9', '\x3', '\x2', '\x2', '\x2', '\xD9', 
		'\xDA', '\a', '\f', '\x2', '\x2', '\xDA', '$', '\x3', '\x2', '\x2', '\x2', 
		'\xDB', '\xDC', '\a', '=', '\x2', '\x2', '\xDC', '&', '\x3', '\x2', '\x2', 
		'\x2', '\xDD', '\xDE', '\x5', ')', '\x15', '\x2', '\xDE', '\xDF', '\x5', 
		'\x61', '\x31', '\x2', '\xDF', '(', '\x3', '\x2', '\x2', '\x2', '\xE0', 
		'\xE1', '\a', '&', '\x2', '\x2', '\xE1', '*', '\x3', '\x2', '\x2', '\x2', 
		'\xE2', '\xE3', '\a', '?', '\x2', '\x2', '\xE3', ',', '\x3', '\x2', '\x2', 
		'\x2', '\xE4', '\xE5', '\a', '-', '\x2', '\x2', '\xE5', '\xE6', '\a', 
		'?', '\x2', '\x2', '\xE6', '.', '\x3', '\x2', '\x2', '\x2', '\xE7', '\xE8', 
		'\a', '/', '\x2', '\x2', '\xE8', '\xE9', '\a', '?', '\x2', '\x2', '\xE9', 
		'\x30', '\x3', '\x2', '\x2', '\x2', '\xEA', '\xEB', '\a', ',', '\x2', 
		'\x2', '\xEB', '\xEC', '\a', '?', '\x2', '\x2', '\xEC', '\x32', '\x3', 
		'\x2', '\x2', '\x2', '\xED', '\xEE', '\a', '\x31', '\x2', '\x2', '\xEE', 
		'\xEF', '\a', '?', '\x2', '\x2', '\xEF', '\x34', '\x3', '\x2', '\x2', 
		'\x2', '\xF0', '\xF1', '\a', '\'', '\x2', '\x2', '\xF1', '\xF2', '\a', 
		'?', '\x2', '\x2', '\xF2', '\x36', '\x3', '\x2', '\x2', '\x2', '\xF3', 
		'\xF4', '\a', '(', '\x2', '\x2', '\xF4', '\xF5', '\a', '(', '\x2', '\x2', 
		'\xF5', '\x38', '\x3', '\x2', '\x2', '\x2', '\xF6', '\xF7', '\a', '~', 
		'\x2', '\x2', '\xF7', '\xF8', '\a', '~', '\x2', '\x2', '\xF8', ':', '\x3', 
		'\x2', '\x2', '\x2', '\xF9', '\xFA', '\a', '?', '\x2', '\x2', '\xFA', 
		'\xFB', '\a', '?', '\x2', '\x2', '\xFB', '<', '\x3', '\x2', '\x2', '\x2', 
		'\xFC', '\xFD', '\a', '#', '\x2', '\x2', '\xFD', '\xFE', '\a', '?', '\x2', 
		'\x2', '\xFE', '>', '\x3', '\x2', '\x2', '\x2', '\xFF', '\x100', '\a', 
		'#', '\x2', '\x2', '\x100', '@', '\x3', '\x2', '\x2', '\x2', '\x101', 
		'\x102', '\a', '@', '\x2', '\x2', '\x102', '\x42', '\x3', '\x2', '\x2', 
		'\x2', '\x103', '\x104', '\a', '>', '\x2', '\x2', '\x104', '\x44', '\x3', 
		'\x2', '\x2', '\x2', '\x105', '\x106', '\a', '@', '\x2', '\x2', '\x106', 
		'\x107', '\a', '?', '\x2', '\x2', '\x107', '\x46', '\x3', '\x2', '\x2', 
		'\x2', '\x108', '\x109', '\a', '>', '\x2', '\x2', '\x109', '\x10A', '\a', 
		'?', '\x2', '\x2', '\x10A', 'H', '\x3', '\x2', '\x2', '\x2', '\x10B', 
		'\x10C', '\a', '(', '\x2', '\x2', '\x10C', 'J', '\x3', '\x2', '\x2', '\x2', 
		'\x10D', '\x10E', '\x5', '\x17', '\f', '\x2', '\x10E', 'L', '\x3', '\x2', 
		'\x2', '\x2', '\x10F', '\x110', '\a', '`', '\x2', '\x2', '\x110', 'N', 
		'\x3', '\x2', '\x2', '\x2', '\x111', '\x112', '\a', '>', '\x2', '\x2', 
		'\x112', '\x113', '\a', '>', '\x2', '\x2', '\x113', 'P', '\x3', '\x2', 
		'\x2', '\x2', '\x114', '\x115', '\a', '@', '\x2', '\x2', '\x115', '\x116', 
		'\a', '@', '\x2', '\x2', '\x116', 'R', '\x3', '\x2', '\x2', '\x2', '\x117', 
		'\x118', '\a', '-', '\x2', '\x2', '\x118', 'T', '\x3', '\x2', '\x2', '\x2', 
		'\x119', '\x11A', '\a', '/', '\x2', '\x2', '\x11A', 'V', '\x3', '\x2', 
		'\x2', '\x2', '\x11B', '\x11C', '\a', ',', '\x2', '\x2', '\x11C', 'X', 
		'\x3', '\x2', '\x2', '\x2', '\x11D', '\x11E', '\a', '\x31', '\x2', '\x2', 
		'\x11E', 'Z', '\x3', '\x2', '\x2', '\x2', '\x11F', '\x120', '\a', '\'', 
		'\x2', '\x2', '\x120', '\\', '\x3', '\x2', '\x2', '\x2', '\x121', '\x122', 
		'\a', '}', '\x2', '\x2', '\x122', '^', '\x3', '\x2', '\x2', '\x2', '\x123', 
		'\x124', '\a', '\x7F', '\x2', '\x2', '\x124', '`', '\x3', '\x2', '\x2', 
		'\x2', '\x125', '\x126', '\a', '*', '\x2', '\x2', '\x126', '\x62', '\x3', 
		'\x2', '\x2', '\x2', '\x127', '\x128', '\a', '+', '\x2', '\x2', '\x128', 
		'\x64', '\x3', '\x2', '\x2', '\x2', '\x129', '\x12B', '\a', '/', '\x2', 
		'\x2', '\x12A', '\x129', '\x3', '\x2', '\x2', '\x2', '\x12A', '\x12B', 
		'\x3', '\x2', '\x2', '\x2', '\x12B', '\x12D', '\x3', '\x2', '\x2', '\x2', 
		'\x12C', '\x12E', '\t', '\x3', '\x2', '\x2', '\x12D', '\x12C', '\x3', 
		'\x2', '\x2', '\x2', '\x12E', '\x12F', '\x3', '\x2', '\x2', '\x2', '\x12F', 
		'\x12D', '\x3', '\x2', '\x2', '\x2', '\x12F', '\x130', '\x3', '\x2', '\x2', 
		'\x2', '\x130', '\x66', '\x3', '\x2', '\x2', '\x2', '\x131', '\x133', 
		'\a', '/', '\x2', '\x2', '\x132', '\x131', '\x3', '\x2', '\x2', '\x2', 
		'\x132', '\x133', '\x3', '\x2', '\x2', '\x2', '\x133', '\x135', '\x3', 
		'\x2', '\x2', '\x2', '\x134', '\x136', '\t', '\x3', '\x2', '\x2', '\x135', 
		'\x134', '\x3', '\x2', '\x2', '\x2', '\x136', '\x137', '\x3', '\x2', '\x2', 
		'\x2', '\x137', '\x135', '\x3', '\x2', '\x2', '\x2', '\x137', '\x138', 
		'\x3', '\x2', '\x2', '\x2', '\x138', '\x139', '\x3', '\x2', '\x2', '\x2', 
		'\x139', '\x13B', '\x5', '\x19', '\r', '\x2', '\x13A', '\x13C', '\t', 
		'\x3', '\x2', '\x2', '\x13B', '\x13A', '\x3', '\x2', '\x2', '\x2', '\x13C', 
		'\x13D', '\x3', '\x2', '\x2', '\x2', '\x13D', '\x13B', '\x3', '\x2', '\x2', 
		'\x2', '\x13D', '\x13E', '\x3', '\x2', '\x2', '\x2', '\x13E', 'h', '\x3', 
		'\x2', '\x2', '\x2', '\x13F', '\x140', '\a', '\x61', '\x2', '\x2', '\x140', 
		'j', '\x3', '\x2', '\x2', '\x2', '\x141', '\x145', '\t', '\x4', '\x2', 
		'\x2', '\x142', '\x144', '\t', '\x5', '\x2', '\x2', '\x143', '\x142', 
		'\x3', '\x2', '\x2', '\x2', '\x144', '\x147', '\x3', '\x2', '\x2', '\x2', 
		'\x145', '\x143', '\x3', '\x2', '\x2', '\x2', '\x145', '\x146', '\x3', 
		'\x2', '\x2', '\x2', '\x146', 'l', '\x3', '\x2', '\x2', '\x2', '\x147', 
		'\x145', '\x3', '\x2', '\x2', '\x2', '\x148', '\x14D', '\a', '$', '\x2', 
		'\x2', '\x149', '\x14C', '\x5', 'o', '\x38', '\x2', '\x14A', '\x14C', 
		'\x5', 'q', '\x39', '\x2', '\x14B', '\x149', '\x3', '\x2', '\x2', '\x2', 
		'\x14B', '\x14A', '\x3', '\x2', '\x2', '\x2', '\x14C', '\x14F', '\x3', 
		'\x2', '\x2', '\x2', '\x14D', '\x14B', '\x3', '\x2', '\x2', '\x2', '\x14D', 
		'\x14E', '\x3', '\x2', '\x2', '\x2', '\x14E', '\x150', '\x3', '\x2', '\x2', 
		'\x2', '\x14F', '\x14D', '\x3', '\x2', '\x2', '\x2', '\x150', '\x151', 
		'\a', '$', '\x2', '\x2', '\x151', 'n', '\x3', '\x2', '\x2', '\x2', '\x152', 
		'\x153', '\n', '\x6', '\x2', '\x2', '\x153', 'p', '\x3', '\x2', '\x2', 
		'\x2', '\x154', '\x155', '\a', '^', '\x2', '\x2', '\x155', '\x156', '\v', 
		'\x2', '\x2', '\x2', '\x156', 'r', '\x3', '\x2', '\x2', '\x2', '\x157', 
		'\x158', '\a', '.', '\x2', '\x2', '\x158', 't', '\x3', '\x2', '\x2', '\x2', 
		'\x159', '\x15A', '\v', '\x2', '\x2', '\x2', '\x15A', 'v', '\x3', '\x2', 
		'\x2', '\x2', '\xF', '\x2', '\xB7', '\xC1', '\xCC', '\xD7', '\x12A', '\x12F', 
		'\x132', '\x137', '\x13D', '\x145', '\x14B', '\x14D', '\x3', '\b', '\x2', 
		'\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
