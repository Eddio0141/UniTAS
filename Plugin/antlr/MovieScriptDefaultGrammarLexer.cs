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
		ROUND_BRACKET_OPEN=46, ROUND_BRACKET_CLOSE=47, INT=48, FLOAT=49, IDENTIFIER_STRING=50, 
		STRING=51, COMMA=52, ANY=53;
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
		"INT", "FLOAT", "IDENTIFIER_STRING", "STRING", "STRING_CHAR", "ESCAPE_SEQUENCE", 
		"COMMA", "ANY"
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
		"'('", "')'", null, null, null, null, "','"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, null, null, null, null, null, null, "WHITESPACE", 
		"COMMENT", "COMMENT_MULTI", "ACTIONSEPARATOR", "NEWLINE", "SEMICOLON", 
		"TUPLE_DECONSTRUCTOR_START", "DOLLAR", "ASSIGN", "PLUS_ASSIGN", "MINUS_ASSIGN", 
		"MULTIPLY_ASSIGN", "DIVIDE_ASSIGN", "MODULO_ASSIGN", "AND", "OR", "EQUAL", 
		"NOT_EQUAL", "NOT", "GREATER", "LESS", "GREATER_EQUAL", "LESS_EQUAL", 
		"BITWISE_AND", "BITWISE_OR", "BITWISE_XOR", "BITWISE_SHIFT_LEFT", "BITWISE_SHIFT_RIGHT", 
		"PLUS", "MINUS", "MULTIPLY", "DIVIDE", "MODULO", "SCOPE_OPEN", "SCOPE_CLOSE", 
		"ROUND_BRACKET_OPEN", "ROUND_BRACKET_CLOSE", "INT", "FLOAT", "IDENTIFIER_STRING", 
		"STRING", "COMMA", "ANY"
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
		'\x5964', '\x2', '\x37', '\x157', '\b', '\x1', '\x4', '\x2', '\t', '\x2', 
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
		'\x4', ':', '\t', ':', '\x3', '\x2', '\x3', '\x2', '\x3', '\x2', '\x3', 
		'\x2', '\x3', '\x2', '\x3', '\x2', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', '\x3', 
		'\x3', '\x3', '\x3', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', 
		'\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x4', '\x3', '\x5', '\x3', 
		'\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x5', '\x3', '\x6', '\x3', 
		'\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', '\x6', '\x3', 
		'\a', '\x3', '\a', '\x3', '\a', '\x3', '\b', '\x3', '\b', '\x3', '\b', 
		'\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', '\b', '\x3', 
		'\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\t', '\x3', '\n', 
		'\x3', '\n', '\x3', '\n', '\x3', '\v', '\x3', '\v', '\x3', '\v', '\x3', 
		'\v', '\x3', '\v', '\x3', '\f', '\x3', '\f', '\x3', '\r', '\x3', '\r', 
		'\x3', '\xE', '\x6', '\xE', '\xB4', '\n', '\xE', '\r', '\xE', '\xE', '\xE', 
		'\xB5', '\x3', '\xE', '\x3', '\xE', '\x3', '\xF', '\x3', '\xF', '\x3', 
		'\xF', '\x3', '\xF', '\a', '\xF', '\xBE', '\n', '\xF', '\f', '\xF', '\xE', 
		'\xF', '\xC1', '\v', '\xF', '\x3', '\xF', '\x3', '\xF', '\x3', '\x10', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\a', '\x10', '\xC9', '\n', 
		'\x10', '\f', '\x10', '\xE', '\x10', '\xCC', '\v', '\x10', '\x3', '\x10', 
		'\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x3', '\x10', '\x3', '\x11', 
		'\x3', '\x11', '\x3', '\x12', '\x5', '\x12', '\xD6', '\n', '\x12', '\x3', 
		'\x12', '\x3', '\x12', '\x3', '\x13', '\x3', '\x13', '\x3', '\x14', '\x3', 
		'\x14', '\x3', '\x14', '\x3', '\x15', '\x3', '\x15', '\x3', '\x16', '\x3', 
		'\x16', '\x3', '\x17', '\x3', '\x17', '\x3', '\x17', '\x3', '\x18', '\x3', 
		'\x18', '\x3', '\x18', '\x3', '\x19', '\x3', '\x19', '\x3', '\x19', '\x3', 
		'\x1A', '\x3', '\x1A', '\x3', '\x1A', '\x3', '\x1B', '\x3', '\x1B', '\x3', 
		'\x1B', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1C', '\x3', '\x1D', '\x3', 
		'\x1D', '\x3', '\x1D', '\x3', '\x1E', '\x3', '\x1E', '\x3', '\x1E', '\x3', 
		'\x1F', '\x3', '\x1F', '\x3', '\x1F', '\x3', ' ', '\x3', ' ', '\x3', '!', 
		'\x3', '!', '\x3', '\"', '\x3', '\"', '\x3', '#', '\x3', '#', '\x3', '#', 
		'\x3', '$', '\x3', '$', '\x3', '$', '\x3', '%', '\x3', '%', '\x3', '&', 
		'\x3', '&', '\x3', '\'', '\x3', '\'', '\x3', '(', '\x3', '(', '\x3', '(', 
		'\x3', ')', '\x3', ')', '\x3', ')', '\x3', '*', '\x3', '*', '\x3', '+', 
		'\x3', '+', '\x3', ',', '\x3', ',', '\x3', '-', '\x3', '-', '\x3', '.', 
		'\x3', '.', '\x3', '/', '\x3', '/', '\x3', '\x30', '\x3', '\x30', '\x3', 
		'\x31', '\x3', '\x31', '\x3', '\x32', '\x3', '\x32', '\x3', '\x33', '\x5', 
		'\x33', '\x129', '\n', '\x33', '\x3', '\x33', '\x6', '\x33', '\x12C', 
		'\n', '\x33', '\r', '\x33', '\xE', '\x33', '\x12D', '\x3', '\x34', '\x5', 
		'\x34', '\x131', '\n', '\x34', '\x3', '\x34', '\x6', '\x34', '\x134', 
		'\n', '\x34', '\r', '\x34', '\xE', '\x34', '\x135', '\x3', '\x34', '\x3', 
		'\x34', '\x6', '\x34', '\x13A', '\n', '\x34', '\r', '\x34', '\xE', '\x34', 
		'\x13B', '\x3', '\x35', '\x3', '\x35', '\a', '\x35', '\x140', '\n', '\x35', 
		'\f', '\x35', '\xE', '\x35', '\x143', '\v', '\x35', '\x3', '\x36', '\x3', 
		'\x36', '\x3', '\x36', '\a', '\x36', '\x148', '\n', '\x36', '\f', '\x36', 
		'\xE', '\x36', '\x14B', '\v', '\x36', '\x3', '\x36', '\x3', '\x36', '\x3', 
		'\x37', '\x3', '\x37', '\x3', '\x38', '\x3', '\x38', '\x3', '\x38', '\x3', 
		'\x39', '\x3', '\x39', '\x3', ':', '\x3', ':', '\x4', '\xBF', '\xCA', 
		'\x2', ';', '\x3', '\x3', '\x5', '\x4', '\a', '\x5', '\t', '\x6', '\v', 
		'\a', '\r', '\b', '\xF', '\t', '\x11', '\n', '\x13', '\v', '\x15', '\f', 
		'\x17', '\x2', '\x19', '\x2', '\x1B', '\r', '\x1D', '\xE', '\x1F', '\xF', 
		'!', '\x10', '#', '\x11', '%', '\x12', '\'', '\x13', ')', '\x14', '+', 
		'\x15', '-', '\x16', '/', '\x17', '\x31', '\x18', '\x33', '\x19', '\x35', 
		'\x1A', '\x37', '\x1B', '\x39', '\x1C', ';', '\x1D', '=', '\x1E', '?', 
		'\x1F', '\x41', ' ', '\x43', '!', '\x45', '\"', 'G', '#', 'I', '$', 'K', 
		'%', 'M', '&', 'O', '\'', 'Q', '(', 'S', ')', 'U', '*', 'W', '+', 'Y', 
		',', '[', '-', ']', '.', '_', '/', '\x61', '\x30', '\x63', '\x31', '\x65', 
		'\x32', 'g', '\x33', 'i', '\x34', 'k', '\x35', 'm', '\x2', 'o', '\x2', 
		'q', '\x36', 's', '\x37', '\x3', '\x2', '\a', '\x4', '\x2', '\v', '\v', 
		'\"', '\"', '\x3', '\x2', '\x32', ';', '\x5', '\x2', '\x43', '\\', '\x61', 
		'\x61', '\x63', '|', '\x6', '\x2', '\x32', ';', '\x43', '\\', '\x61', 
		'\x61', '\x63', '|', '\x6', '\x2', '\f', '\f', '\xF', '\xF', '$', '$', 
		'^', '^', '\x2', '\x15E', '\x2', '\x3', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x5', '\x3', '\x2', '\x2', '\x2', '\x2', '\a', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\t', '\x3', '\x2', '\x2', '\x2', '\x2', '\v', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\r', '\x3', '\x2', '\x2', '\x2', '\x2', '\xF', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x11', '\x3', '\x2', '\x2', '\x2', '\x2', '\x13', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\x15', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '\x1B', '\x3', '\x2', '\x2', '\x2', '\x2', '\x1D', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '\x1F', '\x3', '\x2', '\x2', '\x2', '\x2', '!', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '#', '\x3', '\x2', '\x2', '\x2', '\x2', '%', 
		'\x3', '\x2', '\x2', '\x2', '\x2', '\'', '\x3', '\x2', '\x2', '\x2', '\x2', 
		')', '\x3', '\x2', '\x2', '\x2', '\x2', '+', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '-', '\x3', '\x2', '\x2', '\x2', '\x2', '/', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x31', '\x3', '\x2', '\x2', '\x2', '\x2', '\x33', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x35', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x37', '\x3', '\x2', '\x2', '\x2', '\x2', '\x39', '\x3', '\x2', '\x2', 
		'\x2', '\x2', ';', '\x3', '\x2', '\x2', '\x2', '\x2', '=', '\x3', '\x2', 
		'\x2', '\x2', '\x2', '?', '\x3', '\x2', '\x2', '\x2', '\x2', '\x41', '\x3', 
		'\x2', '\x2', '\x2', '\x2', '\x43', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'\x45', '\x3', '\x2', '\x2', '\x2', '\x2', 'G', '\x3', '\x2', '\x2', '\x2', 
		'\x2', 'I', '\x3', '\x2', '\x2', '\x2', '\x2', 'K', '\x3', '\x2', '\x2', 
		'\x2', '\x2', 'M', '\x3', '\x2', '\x2', '\x2', '\x2', 'O', '\x3', '\x2', 
		'\x2', '\x2', '\x2', 'Q', '\x3', '\x2', '\x2', '\x2', '\x2', 'S', '\x3', 
		'\x2', '\x2', '\x2', '\x2', 'U', '\x3', '\x2', '\x2', '\x2', '\x2', 'W', 
		'\x3', '\x2', '\x2', '\x2', '\x2', 'Y', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'[', '\x3', '\x2', '\x2', '\x2', '\x2', ']', '\x3', '\x2', '\x2', '\x2', 
		'\x2', '_', '\x3', '\x2', '\x2', '\x2', '\x2', '\x61', '\x3', '\x2', '\x2', 
		'\x2', '\x2', '\x63', '\x3', '\x2', '\x2', '\x2', '\x2', '\x65', '\x3', 
		'\x2', '\x2', '\x2', '\x2', 'g', '\x3', '\x2', '\x2', '\x2', '\x2', 'i', 
		'\x3', '\x2', '\x2', '\x2', '\x2', 'k', '\x3', '\x2', '\x2', '\x2', '\x2', 
		'q', '\x3', '\x2', '\x2', '\x2', '\x2', 's', '\x3', '\x2', '\x2', '\x2', 
		'\x3', 'u', '\x3', '\x2', '\x2', '\x2', '\x5', '{', '\x3', '\x2', '\x2', 
		'\x2', '\a', '\x84', '\x3', '\x2', '\x2', '\x2', '\t', '\x8B', '\x3', 
		'\x2', '\x2', '\x2', '\v', '\x90', '\x3', '\x2', '\x2', '\x2', '\r', '\x96', 
		'\x3', '\x2', '\x2', '\x2', '\xF', '\x99', '\x3', '\x2', '\x2', '\x2', 
		'\x11', '\xA1', '\x3', '\x2', '\x2', '\x2', '\x13', '\xA6', '\x3', '\x2', 
		'\x2', '\x2', '\x15', '\xA9', '\x3', '\x2', '\x2', '\x2', '\x17', '\xAE', 
		'\x3', '\x2', '\x2', '\x2', '\x19', '\xB0', '\x3', '\x2', '\x2', '\x2', 
		'\x1B', '\xB3', '\x3', '\x2', '\x2', '\x2', '\x1D', '\xB9', '\x3', '\x2', 
		'\x2', '\x2', '\x1F', '\xC4', '\x3', '\x2', '\x2', '\x2', '!', '\xD2', 
		'\x3', '\x2', '\x2', '\x2', '#', '\xD5', '\x3', '\x2', '\x2', '\x2', '%', 
		'\xD9', '\x3', '\x2', '\x2', '\x2', '\'', '\xDB', '\x3', '\x2', '\x2', 
		'\x2', ')', '\xDE', '\x3', '\x2', '\x2', '\x2', '+', '\xE0', '\x3', '\x2', 
		'\x2', '\x2', '-', '\xE2', '\x3', '\x2', '\x2', '\x2', '/', '\xE5', '\x3', 
		'\x2', '\x2', '\x2', '\x31', '\xE8', '\x3', '\x2', '\x2', '\x2', '\x33', 
		'\xEB', '\x3', '\x2', '\x2', '\x2', '\x35', '\xEE', '\x3', '\x2', '\x2', 
		'\x2', '\x37', '\xF1', '\x3', '\x2', '\x2', '\x2', '\x39', '\xF4', '\x3', 
		'\x2', '\x2', '\x2', ';', '\xF7', '\x3', '\x2', '\x2', '\x2', '=', '\xFA', 
		'\x3', '\x2', '\x2', '\x2', '?', '\xFD', '\x3', '\x2', '\x2', '\x2', '\x41', 
		'\xFF', '\x3', '\x2', '\x2', '\x2', '\x43', '\x101', '\x3', '\x2', '\x2', 
		'\x2', '\x45', '\x103', '\x3', '\x2', '\x2', '\x2', 'G', '\x106', '\x3', 
		'\x2', '\x2', '\x2', 'I', '\x109', '\x3', '\x2', '\x2', '\x2', 'K', '\x10B', 
		'\x3', '\x2', '\x2', '\x2', 'M', '\x10D', '\x3', '\x2', '\x2', '\x2', 
		'O', '\x10F', '\x3', '\x2', '\x2', '\x2', 'Q', '\x112', '\x3', '\x2', 
		'\x2', '\x2', 'S', '\x115', '\x3', '\x2', '\x2', '\x2', 'U', '\x117', 
		'\x3', '\x2', '\x2', '\x2', 'W', '\x119', '\x3', '\x2', '\x2', '\x2', 
		'Y', '\x11B', '\x3', '\x2', '\x2', '\x2', '[', '\x11D', '\x3', '\x2', 
		'\x2', '\x2', ']', '\x11F', '\x3', '\x2', '\x2', '\x2', '_', '\x121', 
		'\x3', '\x2', '\x2', '\x2', '\x61', '\x123', '\x3', '\x2', '\x2', '\x2', 
		'\x63', '\x125', '\x3', '\x2', '\x2', '\x2', '\x65', '\x128', '\x3', '\x2', 
		'\x2', '\x2', 'g', '\x130', '\x3', '\x2', '\x2', '\x2', 'i', '\x13D', 
		'\x3', '\x2', '\x2', '\x2', 'k', '\x144', '\x3', '\x2', '\x2', '\x2', 
		'm', '\x14E', '\x3', '\x2', '\x2', '\x2', 'o', '\x150', '\x3', '\x2', 
		'\x2', '\x2', 'q', '\x153', '\x3', '\x2', '\x2', '\x2', 's', '\x155', 
		'\x3', '\x2', '\x2', '\x2', 'u', 'v', '\a', '\x64', '\x2', '\x2', 'v', 
		'w', '\a', 't', '\x2', '\x2', 'w', 'x', '\a', 'g', '\x2', '\x2', 'x', 
		'y', '\a', '\x63', '\x2', '\x2', 'y', 'z', '\a', 'm', '\x2', '\x2', 'z', 
		'\x4', '\x3', '\x2', '\x2', '\x2', '{', '|', '\a', '\x65', '\x2', '\x2', 
		'|', '}', '\a', 'q', '\x2', '\x2', '}', '~', '\a', 'p', '\x2', '\x2', 
		'~', '\x7F', '\a', 'v', '\x2', '\x2', '\x7F', '\x80', '\a', 'k', '\x2', 
		'\x2', '\x80', '\x81', '\a', 'p', '\x2', '\x2', '\x81', '\x82', '\a', 
		'w', '\x2', '\x2', '\x82', '\x83', '\a', 'g', '\x2', '\x2', '\x83', '\x6', 
		'\x3', '\x2', '\x2', '\x2', '\x84', '\x85', '\a', 't', '\x2', '\x2', '\x85', 
		'\x86', '\a', 'g', '\x2', '\x2', '\x86', '\x87', '\a', 'v', '\x2', '\x2', 
		'\x87', '\x88', '\a', 'w', '\x2', '\x2', '\x88', '\x89', '\a', 't', '\x2', 
		'\x2', '\x89', '\x8A', '\a', 'p', '\x2', '\x2', '\x8A', '\b', '\x3', '\x2', 
		'\x2', '\x2', '\x8B', '\x8C', '\a', 'v', '\x2', '\x2', '\x8C', '\x8D', 
		'\a', 't', '\x2', '\x2', '\x8D', '\x8E', '\a', 'w', '\x2', '\x2', '\x8E', 
		'\x8F', '\a', 'g', '\x2', '\x2', '\x8F', '\n', '\x3', '\x2', '\x2', '\x2', 
		'\x90', '\x91', '\a', 'h', '\x2', '\x2', '\x91', '\x92', '\a', '\x63', 
		'\x2', '\x2', '\x92', '\x93', '\a', 'n', '\x2', '\x2', '\x93', '\x94', 
		'\a', 'u', '\x2', '\x2', '\x94', '\x95', '\a', 'g', '\x2', '\x2', '\x95', 
		'\f', '\x3', '\x2', '\x2', '\x2', '\x96', '\x97', '\a', 'k', '\x2', '\x2', 
		'\x97', '\x98', '\a', 'h', '\x2', '\x2', '\x98', '\xE', '\x3', '\x2', 
		'\x2', '\x2', '\x99', '\x9A', '\a', 'g', '\x2', '\x2', '\x9A', '\x9B', 
		'\a', 'n', '\x2', '\x2', '\x9B', '\x9C', '\a', 'u', '\x2', '\x2', '\x9C', 
		'\x9D', '\a', 'g', '\x2', '\x2', '\x9D', '\x9E', '\a', '\"', '\x2', '\x2', 
		'\x9E', '\x9F', '\a', 'k', '\x2', '\x2', '\x9F', '\xA0', '\a', 'h', '\x2', 
		'\x2', '\xA0', '\x10', '\x3', '\x2', '\x2', '\x2', '\xA1', '\xA2', '\a', 
		'g', '\x2', '\x2', '\xA2', '\xA3', '\a', 'n', '\x2', '\x2', '\xA3', '\xA4', 
		'\a', 'u', '\x2', '\x2', '\xA4', '\xA5', '\a', 'g', '\x2', '\x2', '\xA5', 
		'\x12', '\x3', '\x2', '\x2', '\x2', '\xA6', '\xA7', '\a', 'h', '\x2', 
		'\x2', '\xA7', '\xA8', '\a', 'p', '\x2', '\x2', '\xA8', '\x14', '\x3', 
		'\x2', '\x2', '\x2', '\xA9', '\xAA', '\a', 'n', '\x2', '\x2', '\xAA', 
		'\xAB', '\a', 'q', '\x2', '\x2', '\xAB', '\xAC', '\a', 'q', '\x2', '\x2', 
		'\xAC', '\xAD', '\a', 'r', '\x2', '\x2', '\xAD', '\x16', '\x3', '\x2', 
		'\x2', '\x2', '\xAE', '\xAF', '\a', '~', '\x2', '\x2', '\xAF', '\x18', 
		'\x3', '\x2', '\x2', '\x2', '\xB0', '\xB1', '\a', '\x30', '\x2', '\x2', 
		'\xB1', '\x1A', '\x3', '\x2', '\x2', '\x2', '\xB2', '\xB4', '\t', '\x2', 
		'\x2', '\x2', '\xB3', '\xB2', '\x3', '\x2', '\x2', '\x2', '\xB4', '\xB5', 
		'\x3', '\x2', '\x2', '\x2', '\xB5', '\xB3', '\x3', '\x2', '\x2', '\x2', 
		'\xB5', '\xB6', '\x3', '\x2', '\x2', '\x2', '\xB6', '\xB7', '\x3', '\x2', 
		'\x2', '\x2', '\xB7', '\xB8', '\b', '\xE', '\x2', '\x2', '\xB8', '\x1C', 
		'\x3', '\x2', '\x2', '\x2', '\xB9', '\xBA', '\a', '\x31', '\x2', '\x2', 
		'\xBA', '\xBB', '\a', '\x31', '\x2', '\x2', '\xBB', '\xBF', '\x3', '\x2', 
		'\x2', '\x2', '\xBC', '\xBE', '\v', '\x2', '\x2', '\x2', '\xBD', '\xBC', 
		'\x3', '\x2', '\x2', '\x2', '\xBE', '\xC1', '\x3', '\x2', '\x2', '\x2', 
		'\xBF', '\xC0', '\x3', '\x2', '\x2', '\x2', '\xBF', '\xBD', '\x3', '\x2', 
		'\x2', '\x2', '\xC0', '\xC2', '\x3', '\x2', '\x2', '\x2', '\xC1', '\xBF', 
		'\x3', '\x2', '\x2', '\x2', '\xC2', '\xC3', '\b', '\xF', '\x2', '\x2', 
		'\xC3', '\x1E', '\x3', '\x2', '\x2', '\x2', '\xC4', '\xC5', '\a', '\x31', 
		'\x2', '\x2', '\xC5', '\xC6', '\a', ',', '\x2', '\x2', '\xC6', '\xCA', 
		'\x3', '\x2', '\x2', '\x2', '\xC7', '\xC9', '\v', '\x2', '\x2', '\x2', 
		'\xC8', '\xC7', '\x3', '\x2', '\x2', '\x2', '\xC9', '\xCC', '\x3', '\x2', 
		'\x2', '\x2', '\xCA', '\xCB', '\x3', '\x2', '\x2', '\x2', '\xCA', '\xC8', 
		'\x3', '\x2', '\x2', '\x2', '\xCB', '\xCD', '\x3', '\x2', '\x2', '\x2', 
		'\xCC', '\xCA', '\x3', '\x2', '\x2', '\x2', '\xCD', '\xCE', '\a', ',', 
		'\x2', '\x2', '\xCE', '\xCF', '\a', '\x31', '\x2', '\x2', '\xCF', '\xD0', 
		'\x3', '\x2', '\x2', '\x2', '\xD0', '\xD1', '\b', '\x10', '\x2', '\x2', 
		'\xD1', ' ', '\x3', '\x2', '\x2', '\x2', '\xD2', '\xD3', '\x5', '\x17', 
		'\f', '\x2', '\xD3', '\"', '\x3', '\x2', '\x2', '\x2', '\xD4', '\xD6', 
		'\a', '\xF', '\x2', '\x2', '\xD5', '\xD4', '\x3', '\x2', '\x2', '\x2', 
		'\xD5', '\xD6', '\x3', '\x2', '\x2', '\x2', '\xD6', '\xD7', '\x3', '\x2', 
		'\x2', '\x2', '\xD7', '\xD8', '\a', '\f', '\x2', '\x2', '\xD8', '$', '\x3', 
		'\x2', '\x2', '\x2', '\xD9', '\xDA', '\a', '=', '\x2', '\x2', '\xDA', 
		'&', '\x3', '\x2', '\x2', '\x2', '\xDB', '\xDC', '\x5', ')', '\x15', '\x2', 
		'\xDC', '\xDD', '\x5', '\x61', '\x31', '\x2', '\xDD', '(', '\x3', '\x2', 
		'\x2', '\x2', '\xDE', '\xDF', '\a', '&', '\x2', '\x2', '\xDF', '*', '\x3', 
		'\x2', '\x2', '\x2', '\xE0', '\xE1', '\a', '?', '\x2', '\x2', '\xE1', 
		',', '\x3', '\x2', '\x2', '\x2', '\xE2', '\xE3', '\a', '-', '\x2', '\x2', 
		'\xE3', '\xE4', '\a', '?', '\x2', '\x2', '\xE4', '.', '\x3', '\x2', '\x2', 
		'\x2', '\xE5', '\xE6', '\a', '/', '\x2', '\x2', '\xE6', '\xE7', '\a', 
		'?', '\x2', '\x2', '\xE7', '\x30', '\x3', '\x2', '\x2', '\x2', '\xE8', 
		'\xE9', '\a', ',', '\x2', '\x2', '\xE9', '\xEA', '\a', '?', '\x2', '\x2', 
		'\xEA', '\x32', '\x3', '\x2', '\x2', '\x2', '\xEB', '\xEC', '\a', '\x31', 
		'\x2', '\x2', '\xEC', '\xED', '\a', '?', '\x2', '\x2', '\xED', '\x34', 
		'\x3', '\x2', '\x2', '\x2', '\xEE', '\xEF', '\a', '\'', '\x2', '\x2', 
		'\xEF', '\xF0', '\a', '?', '\x2', '\x2', '\xF0', '\x36', '\x3', '\x2', 
		'\x2', '\x2', '\xF1', '\xF2', '\a', '(', '\x2', '\x2', '\xF2', '\xF3', 
		'\a', '(', '\x2', '\x2', '\xF3', '\x38', '\x3', '\x2', '\x2', '\x2', '\xF4', 
		'\xF5', '\a', '~', '\x2', '\x2', '\xF5', '\xF6', '\a', '~', '\x2', '\x2', 
		'\xF6', ':', '\x3', '\x2', '\x2', '\x2', '\xF7', '\xF8', '\a', '?', '\x2', 
		'\x2', '\xF8', '\xF9', '\a', '?', '\x2', '\x2', '\xF9', '<', '\x3', '\x2', 
		'\x2', '\x2', '\xFA', '\xFB', '\a', '#', '\x2', '\x2', '\xFB', '\xFC', 
		'\a', '?', '\x2', '\x2', '\xFC', '>', '\x3', '\x2', '\x2', '\x2', '\xFD', 
		'\xFE', '\a', '#', '\x2', '\x2', '\xFE', '@', '\x3', '\x2', '\x2', '\x2', 
		'\xFF', '\x100', '\a', '@', '\x2', '\x2', '\x100', '\x42', '\x3', '\x2', 
		'\x2', '\x2', '\x101', '\x102', '\a', '>', '\x2', '\x2', '\x102', '\x44', 
		'\x3', '\x2', '\x2', '\x2', '\x103', '\x104', '\a', '@', '\x2', '\x2', 
		'\x104', '\x105', '\a', '?', '\x2', '\x2', '\x105', '\x46', '\x3', '\x2', 
		'\x2', '\x2', '\x106', '\x107', '\a', '>', '\x2', '\x2', '\x107', '\x108', 
		'\a', '?', '\x2', '\x2', '\x108', 'H', '\x3', '\x2', '\x2', '\x2', '\x109', 
		'\x10A', '\a', '(', '\x2', '\x2', '\x10A', 'J', '\x3', '\x2', '\x2', '\x2', 
		'\x10B', '\x10C', '\x5', '\x17', '\f', '\x2', '\x10C', 'L', '\x3', '\x2', 
		'\x2', '\x2', '\x10D', '\x10E', '\a', '`', '\x2', '\x2', '\x10E', 'N', 
		'\x3', '\x2', '\x2', '\x2', '\x10F', '\x110', '\a', '>', '\x2', '\x2', 
		'\x110', '\x111', '\a', '>', '\x2', '\x2', '\x111', 'P', '\x3', '\x2', 
		'\x2', '\x2', '\x112', '\x113', '\a', '@', '\x2', '\x2', '\x113', '\x114', 
		'\a', '@', '\x2', '\x2', '\x114', 'R', '\x3', '\x2', '\x2', '\x2', '\x115', 
		'\x116', '\a', '-', '\x2', '\x2', '\x116', 'T', '\x3', '\x2', '\x2', '\x2', 
		'\x117', '\x118', '\a', '/', '\x2', '\x2', '\x118', 'V', '\x3', '\x2', 
		'\x2', '\x2', '\x119', '\x11A', '\a', ',', '\x2', '\x2', '\x11A', 'X', 
		'\x3', '\x2', '\x2', '\x2', '\x11B', '\x11C', '\a', '\x31', '\x2', '\x2', 
		'\x11C', 'Z', '\x3', '\x2', '\x2', '\x2', '\x11D', '\x11E', '\a', '\'', 
		'\x2', '\x2', '\x11E', '\\', '\x3', '\x2', '\x2', '\x2', '\x11F', '\x120', 
		'\a', '}', '\x2', '\x2', '\x120', '^', '\x3', '\x2', '\x2', '\x2', '\x121', 
		'\x122', '\a', '\x7F', '\x2', '\x2', '\x122', '`', '\x3', '\x2', '\x2', 
		'\x2', '\x123', '\x124', '\a', '*', '\x2', '\x2', '\x124', '\x62', '\x3', 
		'\x2', '\x2', '\x2', '\x125', '\x126', '\a', '+', '\x2', '\x2', '\x126', 
		'\x64', '\x3', '\x2', '\x2', '\x2', '\x127', '\x129', '\a', '/', '\x2', 
		'\x2', '\x128', '\x127', '\x3', '\x2', '\x2', '\x2', '\x128', '\x129', 
		'\x3', '\x2', '\x2', '\x2', '\x129', '\x12B', '\x3', '\x2', '\x2', '\x2', 
		'\x12A', '\x12C', '\t', '\x3', '\x2', '\x2', '\x12B', '\x12A', '\x3', 
		'\x2', '\x2', '\x2', '\x12C', '\x12D', '\x3', '\x2', '\x2', '\x2', '\x12D', 
		'\x12B', '\x3', '\x2', '\x2', '\x2', '\x12D', '\x12E', '\x3', '\x2', '\x2', 
		'\x2', '\x12E', '\x66', '\x3', '\x2', '\x2', '\x2', '\x12F', '\x131', 
		'\a', '/', '\x2', '\x2', '\x130', '\x12F', '\x3', '\x2', '\x2', '\x2', 
		'\x130', '\x131', '\x3', '\x2', '\x2', '\x2', '\x131', '\x133', '\x3', 
		'\x2', '\x2', '\x2', '\x132', '\x134', '\t', '\x3', '\x2', '\x2', '\x133', 
		'\x132', '\x3', '\x2', '\x2', '\x2', '\x134', '\x135', '\x3', '\x2', '\x2', 
		'\x2', '\x135', '\x133', '\x3', '\x2', '\x2', '\x2', '\x135', '\x136', 
		'\x3', '\x2', '\x2', '\x2', '\x136', '\x137', '\x3', '\x2', '\x2', '\x2', 
		'\x137', '\x139', '\x5', '\x19', '\r', '\x2', '\x138', '\x13A', '\t', 
		'\x3', '\x2', '\x2', '\x139', '\x138', '\x3', '\x2', '\x2', '\x2', '\x13A', 
		'\x13B', '\x3', '\x2', '\x2', '\x2', '\x13B', '\x139', '\x3', '\x2', '\x2', 
		'\x2', '\x13B', '\x13C', '\x3', '\x2', '\x2', '\x2', '\x13C', 'h', '\x3', 
		'\x2', '\x2', '\x2', '\x13D', '\x141', '\t', '\x4', '\x2', '\x2', '\x13E', 
		'\x140', '\t', '\x5', '\x2', '\x2', '\x13F', '\x13E', '\x3', '\x2', '\x2', 
		'\x2', '\x140', '\x143', '\x3', '\x2', '\x2', '\x2', '\x141', '\x13F', 
		'\x3', '\x2', '\x2', '\x2', '\x141', '\x142', '\x3', '\x2', '\x2', '\x2', 
		'\x142', 'j', '\x3', '\x2', '\x2', '\x2', '\x143', '\x141', '\x3', '\x2', 
		'\x2', '\x2', '\x144', '\x149', '\a', '$', '\x2', '\x2', '\x145', '\x148', 
		'\x5', 'm', '\x37', '\x2', '\x146', '\x148', '\x5', 'o', '\x38', '\x2', 
		'\x147', '\x145', '\x3', '\x2', '\x2', '\x2', '\x147', '\x146', '\x3', 
		'\x2', '\x2', '\x2', '\x148', '\x14B', '\x3', '\x2', '\x2', '\x2', '\x149', 
		'\x147', '\x3', '\x2', '\x2', '\x2', '\x149', '\x14A', '\x3', '\x2', '\x2', 
		'\x2', '\x14A', '\x14C', '\x3', '\x2', '\x2', '\x2', '\x14B', '\x149', 
		'\x3', '\x2', '\x2', '\x2', '\x14C', '\x14D', '\a', '$', '\x2', '\x2', 
		'\x14D', 'l', '\x3', '\x2', '\x2', '\x2', '\x14E', '\x14F', '\n', '\x6', 
		'\x2', '\x2', '\x14F', 'n', '\x3', '\x2', '\x2', '\x2', '\x150', '\x151', 
		'\a', '^', '\x2', '\x2', '\x151', '\x152', '\v', '\x2', '\x2', '\x2', 
		'\x152', 'p', '\x3', '\x2', '\x2', '\x2', '\x153', '\x154', '\a', '.', 
		'\x2', '\x2', '\x154', 'r', '\x3', '\x2', '\x2', '\x2', '\x155', '\x156', 
		'\v', '\x2', '\x2', '\x2', '\x156', 't', '\x3', '\x2', '\x2', '\x2', '\xF', 
		'\x2', '\xB5', '\xBF', '\xCA', '\xD5', '\x128', '\x12D', '\x130', '\x135', 
		'\x13B', '\x141', '\x147', '\x149', '\x3', '\b', '\x2', '\x2',
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
