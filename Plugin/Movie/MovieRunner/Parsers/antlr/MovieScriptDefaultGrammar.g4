grammar MovieScriptDefaultGrammar;

/*
 * Parser rules
 */

script: program NEWLINE* EOF;

program:
	NEWLINE+ program
	| (actionWithSeparator actionSeparator program)
	| action program
	| (actionWithSeparator actionSeparator?)
	| action;
scopedProgram: SCOPE_OPEN program? NEWLINE* SCOPE_CLOSE;

actionSeparator: ACTIONSEPARATOR | NEWLINE | frameAdvance;

action:
	frameAdvance
	| ifStatement
	| methodDef
	| scopedProgram
	| loop
	| breakAction
	| continueAction
	| returnAction;
actionWithSeparator:
	variableAssignment
	| tupleAssignment
	| variableTupleSeparation
	| methodCall;

frameAdvance: SEMICOLON;

breakAction: 'break';

continueAction: 'continue';

returnAction: 'return' (expression | tupleExpression)?;

variable: DOLLAR IDENTIFIER_STRING;

variableAssignment:
	variable (
		ASSIGN
		| PLUS_ASSIGN
		| MINUS_ASSIGN
		| MULTIPLY_ASSIGN
		| DIVIDE_ASSIGN
		| MODULO_ASSIGN
	) expression;
tupleAssignment: variable ASSIGN tupleExpression;

variableTupleSeparation:
	TUPLE_DECONSTRUCTOR_START (firstVar = tupleVar) (
		COMMA vars += tupleVar
	)* ROUND_BRACKET_CLOSE ASSIGN (
		tupleExpression
		| methodCall
		| variable
	);

tupleVar:
	varIgnore = IGNORE_VARIABLE_NAME
	| varName = IDENTIFIER_STRING;

tupleExpression:
	ROUND_BRACKET_OPEN (expression | tupleExpression) (
		COMMA (expression | tupleExpression)
	)+ ROUND_BRACKET_CLOSE;

expression:
	ROUND_BRACKET_OPEN basicType ROUND_BRACKET_CLOSE expression			# castExpression
	| MINUS expression													# flipSign
	| left = expression (MULTIPLY | DIVIDE | MODULO) right = expression	# multiplyDivide
	| left = expression (PLUS | MINUS) right = expression				# addSubtract
	| NOT expression													# not
	| expression (AND | OR) expression									# andOr
	| expression (
		EQUAL
		| NOT_EQUAL
		| LESS
		| LESS_EQUAL
		| GREATER
		| GREATER_EQUAL
	) expression														# compare
	| expression (BITWISE_AND | BITWISE_OR | BITWISE_XOR) expression	# bitwise
	| expression (BITWISE_SHIFT_LEFT | BITWISE_SHIFT_RIGHT) expression	# bitwiseShift
	| ROUND_BRACKET_OPEN expression ROUND_BRACKET_CLOSE					# parentheses
	| (
		variable
		| intType
		| floatType
		| bool
		| string
		| methodCall
	) # terminator;

basicType:
	toInt = 'int'
	| toFloat = 'float'
	| toBool = 'bool'
	| toString = 'string';

string: STRING;
intType: INT;
floatType: FLOAT;

bool: 'true' | 'false';

ifStatement:
	'if' expression scopedProgram (
		elseIfStatement
		| elseStatement
	)?;
elseIfStatement:
	'else if' expression scopedProgram (
		elseIfStatement
		| elseStatement
	)?;
elseStatement: 'else' scopedProgram;

methodCall:
	IDENTIFIER_STRING ROUND_BRACKET_OPEN methodCallArgs? ROUND_BRACKET_CLOSE;

methodCallArgs: (expression | tupleExpression) COMMA methodCallArgs
	| (expression | tupleExpression);

methodDef:
	'fn' IDENTIFIER_STRING ROUND_BRACKET_OPEN methodDefArgs? ROUND_BRACKET_CLOSE scopedProgram;

methodDefArgs:
	IDENTIFIER_STRING COMMA methodDefArgs
	| IDENTIFIER_STRING;

loop: 'loop' expression scopedProgram;

/*
 * Lexer rules
 */

WHITESPACE: (' ' | '\t')+ -> skip;
COMMENT: '//' ~('\r' | '\n')* -> skip;
COMMENT_MULTI: '/*' .*? '*/' -> skip;

fragment PIPE: '|';
fragment DOT: '.';

ACTIONSEPARATOR: PIPE;

NEWLINE: ('\r'? '\n');

SEMICOLON: ';';

TUPLE_DECONSTRUCTOR_START: DOLLAR ROUND_BRACKET_OPEN;

DOLLAR: '$';

ASSIGN: '=';
PLUS_ASSIGN: '+=';
MINUS_ASSIGN: '-=';
MULTIPLY_ASSIGN: '*=';
DIVIDE_ASSIGN: '/=';
MODULO_ASSIGN: '%=';

AND: '&&';
OR: '||';
EQUAL: '==';
NOT_EQUAL: '!=';
NOT: '!';
GREATER: '>';
LESS: '<';
GREATER_EQUAL: '>=';
LESS_EQUAL: '<=';

BITWISE_AND: '&';
BITWISE_OR: PIPE;
BITWISE_XOR: '^';
BITWISE_SHIFT_LEFT: '<<';
BITWISE_SHIFT_RIGHT: '>>';

PLUS: '+';
MINUS: '-';
MULTIPLY: '*';
DIVIDE: '/';
MODULO: '%';

SCOPE_OPEN: '{';
SCOPE_CLOSE: '}';
ROUND_BRACKET_OPEN: '(';
ROUND_BRACKET_CLOSE: ')';

INT: '-'? [0-9]+;
FLOAT: '-'? [0-9]+ DOT [0-9]+;
IGNORE_VARIABLE_NAME: '_';
IDENTIFIER_STRING: [a-zA-Z_][a-zA-Z0-9_]*;

STRING: '"' (STRING_CHAR | ESCAPE_SEQUENCE)* '"';
fragment STRING_CHAR: ~[\r\n"\\];
fragment ESCAPE_SEQUENCE: '\\' .;

COMMA: ',';

ANY: .;