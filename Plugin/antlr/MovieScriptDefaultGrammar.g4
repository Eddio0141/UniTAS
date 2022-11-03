grammar MovieScriptDefaultGrammar;

/*
 * Parser rules
 */

program: (actionWithSeparator actionSeparator | action NEWLINE*)*;

actionSeparator: ACTIONSEPARATOR | NEWLINE | frameAdvance;

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

scopeOpen: SCOPE_OPEN;
scopeClose: SCOPE_CLOSE;

actionWithSeparator
    : variableAssignment
    | variableTupleSeparation
    | methodCall
    ;

frameAdvance: SEMICOLON;

breakAction: 'break';

continueAction: 'continue';

returnAction: 'return' (expression | tupleExpression)?;

variable: DOLLAR IDENTIFIER_STRING;

variableAssignment: variable (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (expression | tupleExpression);

variableTupleSeparation: ROUND_BRACKET_OPEN variable (COMMA variable)* ROUND_BRACKET_CLOSE (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (tupleExpression | methodCall);

tupleExpression: ROUND_BRACKET_OPEN (expression | tupleExpression) (COMMA (expression | tupleExpression))+ ROUND_BRACKET_CLOSE;

expression
    : ROUND_BRACKET_OPEN expression ROUND_BRACKET_CLOSE
    | expression (MULTIPLY | DIVIDE | MODULO) expression
    | expression (PLUS | MINUS) expression
    | MINUS expression
    | NOT expression
    | expression (AND | OR) expression
    | expression (EQUAL | NOT_EQUAL | LESS | LESS_EQUAL | GREATER | GREATER_EQUAL) expression
    | expression (BITWISE_AND | BITWISE_OR | BITWISE_XOR) expression
    | expression (BITWISE_SHIFT_LEFT | BITWISE_SHIFT_RIGHT) expression
    | expressionTerminator
    ;

expressionTerminator
    : variable
    | intType
    | floatType
    | bool
    | string
    | methodCall
    ;
string: STRING;
intType: INT;
floatType: FLOAT;

bool: 'true' | 'false';

ifElse: 'if' expression SCOPE_OPEN NEWLINE* program SCOPE_CLOSE ('else if' expression SCOPE_OPEN NEWLINE* program SCOPE_CLOSE)* ('else' SCOPE_OPEN NEWLINE* program SCOPE_CLOSE)?;

methodCall: IDENTIFIER_STRING ROUND_BRACKET_OPEN methodCallArgs? ROUND_BRACKET_CLOSE;

methodCallArgs: expression COMMA methodCallArgs | expression;

methodDef: 'fn' IDENTIFIER_STRING ROUND_BRACKET_OPEN methodDefArgs? ROUND_BRACKET_CLOSE SCOPE_OPEN NEWLINE* program SCOPE_CLOSE;

methodDefArgs: IDENTIFIER_STRING COMMA methodDefArgs | IDENTIFIER_STRING;

loop: 'loop' ROUND_BRACKET_OPEN expression ROUND_BRACKET_CLOSE SCOPE_OPEN NEWLINE* program SCOPE_CLOSE;

/*
 * Lexer rules
 */
 
fragment PIPE: '|';
fragment DOT: '.';

WHITESPACE : (' ' | '\t')+ -> skip;
COMMENT: '//' .*? -> skip;
COMMENT_MULTI: '/*' .*? '*/' -> skip;

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

INT: '-'? [0-9]+;
FLOAT: '-'? [0-9]+ DOT [0-9]+;
IDENTIFIER_STRING: [a-zA-Z_][a-zA-Z0-9_]*;

STRING: '"' (STRING_CHAR | ESCAPE_SEQUENCE)* '"';
fragment STRING_CHAR: ~[\r\n"\\];
fragment ESCAPE_SEQUENCE: '\\' .;

COMMA: ',';