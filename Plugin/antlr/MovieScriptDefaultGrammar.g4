grammar MovieScriptDefaultGrammar;

/*
 * Parser rules
 */

script: program EOF;
program: (actionWithSeparator actionSeparator?) | (action NEWLINE*) | (actionWithSeparator actionSeparator program) | (action NEWLINE* program);
scopedProgram: SCOPE_OPEN NEWLINE* program? NEWLINE* SCOPE_CLOSE;

actionSeparator: ACTIONSEPARATOR | NEWLINE | frameAdvance;

action
    : frameAdvance
    | ifStatement
    | methodDef
    | scopedProgram
    | loop
    | breakAction
    | continueAction
    | returnAction
    ;

actionWithSeparator
    : variableAssignment
    | tupleAssignment
    | variableTupleSeparation
    | methodCall
    ;

frameAdvance: SEMICOLON;

breakAction: 'break';

continueAction: 'continue';

returnAction: 'return' (expression | tupleExpression)?;

variable: DOLLAR IDENTIFIER_STRING;

variableAssignment: variable (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) expression;
tupleAssignment: variable ASSIGN tupleExpression;

variableTupleSeparation: TUPLE_DECONSTRUCTOR_START varName=IDENTIFIER_STRING (COMMA varNames+=IDENTIFIER_STRING)* ROUND_BRACKET_CLOSE ASSIGN (tupleExpression | methodCall | variable);

tupleExpression: ROUND_BRACKET_OPEN (expression | tupleExpression) (COMMA (expression | tupleExpression))+ ROUND_BRACKET_CLOSE;

expression
    : MINUS expression #flipSign
    | left=expression (MULTIPLY | DIVIDE | MODULO) right=expression #multiplyDivide
    | left=expression (PLUS | MINUS) right=expression #addSubtract
    | NOT expression #not
    | expression (AND | OR) expression #andOr
    | expression (EQUAL | NOT_EQUAL | LESS | LESS_EQUAL | GREATER | GREATER_EQUAL) expression #compare
    | expression (BITWISE_AND | BITWISE_OR | BITWISE_XOR) expression #bitwise
    | expression (BITWISE_SHIFT_LEFT | BITWISE_SHIFT_RIGHT) expression #bitwiseShift
    | ROUND_BRACKET_OPEN expression ROUND_BRACKET_CLOSE #parentheses
    | (variable | intType | floatType | bool | string | methodCall) #terminator
    ;

string: STRING;
intType: INT;
floatType: FLOAT;

bool: 'true' | 'false';

ifStatement: 'if' expression scopedProgram (elseIfStatement | elseStatement)?;
elseIfStatement: 'else if' expression scopedProgram (elseIfStatement | elseStatement)?;
elseStatement: 'else' scopedProgram;

methodCall: IDENTIFIER_STRING ROUND_BRACKET_OPEN methodCallArgs? ROUND_BRACKET_CLOSE;

methodCallArgs: (expression | tupleExpression) COMMA methodCallArgs | (expression | tupleExpression);

methodDef: 'fn' IDENTIFIER_STRING ROUND_BRACKET_OPEN methodDefArgs? ROUND_BRACKET_CLOSE scopedProgram;

methodDefArgs: IDENTIFIER_STRING COMMA methodDefArgs | IDENTIFIER_STRING;

loop: 'loop' expression scopedProgram;

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
IDENTIFIER_STRING: [a-zA-Z_][a-zA-Z0-9_]*;

STRING: '"' (STRING_CHAR | ESCAPE_SEQUENCE)* '"';
fragment STRING_CHAR: ~[\r\n"\\];
fragment ESCAPE_SEQUENCE: '\\' .;

COMMA: ',';

ANY: .;