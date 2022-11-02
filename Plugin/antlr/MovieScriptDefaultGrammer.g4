grammar MovieScriptDefaultGrammer;

/*
 * Parser rules
 */

program
    : (actionWithSeparator actionSeparator | action NEWLINE?) program
    | (actionWithSeparator | action NEWLINE?)
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

breakAction: BREAK;

continueAction: CONTINUE;

returnAction: RETURN (expression | tupleExpression)?;

variable: DOLLAR stringIdentifier;

variableAssignment: variable (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (expression | tupleExpression);

variableTupleSeparation: roundBracketOpen variable (COMMA variable)* roundBracketClose (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (tupleExpression | methodCall);

tupleExpression: roundBracketOpen expression (COMMA expression)* roundBracketClose;

expression
    : expression binaryOpType expression
    | expression mathOpType expression
    | expression logicOpType expression
    | variable
    | intValue
    | floatValue
    | boolValue
    | string
    | methodCall
    ;

mathOpType: PLUS | MINUS | MULTIPLY | DIVIDE | MODULO;

logicOpType: AND | OR | EQUAL | NOT_EQUAL | NOT | GREATER | LESS | GREATER_EQUAL | LESS_EQUAL;

binaryOpType: AND_BINARY | OR_BINARY | XOR_BINARY;

intValue: intDigit intValue | intDigit;

intDigit: NUMBER;

floatValue: floatDigit floatValue | floatDigit;

floatDigit: NUMBER | DOT;

boolValue: TRUE | FALSE;

string: stringLiteral;

ifElse: IF expression scopeOpen program scopeClose (ELSE_IF expression scopeOpen program scopeClose)* (ELSE scopeOpen program scopeClose)?;

methodCall: methodName roundBracketOpen methodCallArgs roundBracketClose;

methodCallArgs: expression methodCallArgsSeparator methodCallArgs | expression;

methodCallArgsSeparator: COMMA;

methodDef: FN methodName roundBracketOpen methodDefArgs roundBracketClose scopeOpen program scopeClose;

methodName: stringIdentifier;

methodDefArgs: stringIdentifier methodDefArgsSeparator methodDefArgs | stringIdentifier;

methodDefArgsSeparator: COMMA;

scopeOpen: SCOPE_OPEN;

scopeClose: SCOPE_CLOSE;

loop: LOOP roundBracketOpen expression roundBracketClose scopeOpen program scopeClose;

roundBracketOpen: ROUND_BRACKET_OPEN;

roundBracketClose: ROUND_BRACKET_CLOSE;

squareBracketOpen: SQUARE_BRACKET_OPEN;

squareBracketClose: SQUARE_BRACKET_CLOSE;

stringIdentifier: IDENTIFIER_STRING;

stringLiteral: STRING_LITERAL;

stringChar: STRING_CHAR;

/*
 * Lexer rules
 */
 
fragment PIPE: '|';

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

BREAK: 'break';

CONTINUE: 'continue';

RETURN: 'return';

IF: 'if';

ELSE_IF: 'else if';

ELSE: 'else';

FN: 'fn';

LOOP: 'loop';

AND: '&&';
OR: '||';
EQUAL: '==';
NOT_EQUAL: '!=';
NOT: '!';
GREATER: '>';
LESS: '<';
GREATER_EQUAL: '>=';
LESS_EQUAL: '<=';

AND_BINARY: '&';
OR_BINARY: PIPE;
XOR_BINARY: '^';

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

NUMBER: [0-9];
DOT: '.';
IDENTIFIER_STRING: [a-zA-Z_][a-zA-Z0-9_]*;
TRUE: 'true';
FALSE: 'false';
STRING_LITERAL: '"' (STRING_CHAR | ESCAPE_SEQUENCE)* '"';

STRING_CHAR: [^"\\];
ESCAPE_SEQUENCE: '\\' [btnfr"'\\];

COMMA: ',';

WHITESPACE : (' ' | '\t')+ -> skip;
COMMENT: '//' .*? -> skip;
COMMENT_MULTI: '/*' .*? '*/' -> skip;