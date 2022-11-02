grammar MovieScriptDefaultGrammer;

/*
 * Parser rules
 */

// newline between actions are ignored
program
    : (actionWithSeparator actionSeparator | action NEWLINE*) program
    | (actionWithSeparator | action NEWLINE*)
    | EOF
    ;

actionSeparator: ACTIONSEPARATOR | NEWLINE | SEMICOLON;

action
    : frameAdvance
    | ifElse
    | methodDef
    | scopeOpen
    | scopeClose
    | loop
    | breakAction
    | continueAction
    | returnAction
    ;

actionWithSeparator
    : variableAssignment
    | variableTupleSeparation
    | methodCall
    ;

frameAdvance: SEMICOLON;

breakAction: 'break';

continueAction: 'continue';

returnAction: 'return' (expression | tupleExpression)?;

variable: DOLLAR stringIdentifier;

variableAssignment: variable (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (expression | tupleExpression);

variableTupleSeparation: roundBracketOpen variable (COMMA variable)* roundBracketClose (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (tupleExpression | methodCall);

tupleExpression: roundBracketOpen expression (COMMA expression)* roundBracketClose;

expression
    : roundBracketOpen expression roundBracketClose
    | expression (MULTIPLY | DIVIDE | MODULO) expression
    | expression (PLUS | MINUS) expression
    | MINUS expression
    | NOT expression
    | expression (AND | OR) expression
    | expression (EQUAL | NOT_EQUAL | LESS | LESS_EQUAL | GREATER | GREATER_EQUAL) expression
    | expression (BITWISE_AND | BITWISE_OR | BITWISE_XOR) expression
    | expression (BITWISE_SHIFT_LEFT | BITWISE_SHIFT_RIGHT) expression
    | variable
    | intType
    | floatType
    | bool
    | string
    | methodCall
    ;

string: STRING_LITERAL;
intType: INT;
floatType: FLOAT;

bool: 'true' | 'false';

ifElse: 'if' expression scopeOpen program scopeClose ('else if' expression scopeOpen program scopeClose)* ('else' scopeOpen program scopeClose)?;

methodCall: methodName roundBracketOpen methodCallArgs roundBracketClose;

methodCallArgs: expression methodCallArgsSeparator methodCallArgs | expression;

methodCallArgsSeparator: COMMA;

methodDef: 'fn' methodName roundBracketOpen methodDefArgs roundBracketClose scopeOpen program scopeClose;

methodName: stringIdentifier;

methodDefArgs: stringIdentifier methodDefArgsSeparator methodDefArgs | stringIdentifier;

methodDefArgsSeparator: COMMA;

scopeOpen: SCOPE_OPEN;

scopeClose: SCOPE_CLOSE;

loop: 'loop' roundBracketOpen expression roundBracketClose scopeOpen program scopeClose;

roundBracketOpen: ROUND_BRACKET_OPEN;

roundBracketClose: ROUND_BRACKET_CLOSE;

squareBracketOpen: SQUARE_BRACKET_OPEN;

squareBracketClose: SQUARE_BRACKET_CLOSE;

stringIdentifier: IDENTIFIER_STRING;

stringChar: STRING_CHAR;

/*
 * Lexer rules
 */
 
fragment PIPE: '|';
fragment DOT: '.';

ACTIONSEPARATOR: PIPE;

NEWLINE: ('\r'? '\n');

SEMICOLON: ';';

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
SQUARE_BRACKET_OPEN: '[';
SQUARE_BRACKET_CLOSE: ']';

INT: '-'? [0-9]+;
FLOAT: '-'? [0-9]+ DOT [0-9]+;
IDENTIFIER_STRING: [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL: '"' (STRING_CHAR | ESCAPE_SEQUENCE)* '"';

STRING_CHAR: [^"\\];
ESCAPE_SEQUENCE: '\\' [btnfr"'\\];

COMMA: ',';

WHITESPACE : (' ' | '\t')+ -> skip;
COMMENT: '//' .*? -> skip;
COMMENT_MULTI: '/*' .*? '*/' -> skip;