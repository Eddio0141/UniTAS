/*
BNF reference

<string_identifier> = "a" .. "z" | "A" .. "Z" | "0" .. "9" | "_" | "-"
<char> = "UTF-8"

<program> = (<action_with_separator> <action_separator> | <action>) <program> | (<action_with_separator> | <action>)
<action_separator> = "|" | "\n" | <frame_advance>
<action> = <frame_advance> | <if_else> | <method_def> | <scope_open> | <scope_close> | <loop> | <comment> | <break> | <continue> | <return>
<action_with_separator> = <variable_assignment> | <variable_tuple_separation> | <method_call>
<frame_advance> = ";"
<break> = "break"
<continue> = "continue"
<return> = "return" [ ( <expression> | <tuple_expression> ) ]
<variable> = "$" <string_identifier>
<variable_assignment> = <variable> ("=" | "+=" | "-=" | "*=" | "/=" | "%=") (<expression> | <tuple_expression>)
<variable_tuple_separation> = "(" <variable> { "," <variable> } ")" ("=" | "+=" | "-=" | "*=" | "/=" | "%=") ( <tuple_expression> | <method_call> )
<tuple_expression> = "(" <expression> { "," <expression> } ")"
<expression> = <binary_op> | <math_op> | <logic_op> | <variable> | <int> | <float> | <string> | <method_call>
<math_op> = <expression> <math_op_type> <expression>
<math_op_type> = "+" | "-" | "*" | "/" | "%"
<logic_op> = <expression> <logic_op_type> <expression>
<logic_op_type> = "&&" | "||" | "==" | "!=" | "!" | ">" | "<" | ">=" | "<="
<binary_op> = <expression> <binary_op_type> <expression>
<binary_op_type> = "&" | "|" | "^"
<int> = <int_value>
<int_value> = <int_digit> <int_value> | <int_digit>
<int_digit> = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
<float> = <float_value>
<float_value> = <float_digit> <float_value> | <float_digit>
<float_digit> = <int_digit> | "."
<string> = <string_value>
<string_value> = ( <string_char> | <escape_char> ) <string_value> | ( <string_char> | <escape_char> )
<escape_char> = "\" <escape_char_value>
<escape_char_value> = "n" | "t" | "r" | "0" | "\"" | "\\" 
<if_else> = "if" <expression> <scope_open> <program> <scope_close> { "else if" <expression> <scope_open> <program> <scope_close> } [ "else" <scope_open> <program> <scope_close> ]
<method_call> = <method_name> <round_bracket_open> <method_call_args> <round_bracket_close>
<method_call_args> = <expression> <method_call_args_separator> <method_call_args> | <expression>
<method_call_args_separator> = ","
<method_def> = "fn" <method_name> <round_bracket_open> <method_def_args> <round_bracket_close> <scope_open> <program> <scope_close>
<method_name> = <string_identifier>
<method_def_args> = <string_identifier> <method_def_args_separator> <method_def_args> | <string_identifier>
<method_def_args_separator> = ","
<scope_open> = "{"
<scope_close> = "}"
<loop> = "loop" <round_bracket_open> <expression> <round_bracket_close> <scope_open> <program> <scope_close>
<comment> = "//" <string_identifier> | "/*" <string_identifier> "*/ /*
<round_bracket_open> = "("
<round_bracket_close> = ")"
<square_bracket_open> = "["
<square_bracket_close> = "]"
*/

grammar MovieScriptDefaultGrammer;

/*
 * Parser rules
 */

program
    : (actionWithSeparator actionSeparator | action) program
    | (actionWithSeparator | action)
    ;

actionSeparator: PIPE | NEWLINE | SEMICOLON;

action
    : frameAdvance
    | ifElse
    | methodDef
    | scopeOpen
    | scopeClose
    | loop
    | break
    | continue
    | return
    ;

actionWithSeparator
    : variableAssignment
    | variableTupleSeparation
    | methodCall
    ;

frameAdvance: SEMICOLON;

break: BREAK;

continue: CONTINUE;

return: RETURN (expression | tupleExpression)?;

variable: DOLLAR stringIdentifier;

variableAssignment: variable (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (expression | tupleExpression);

variableTupleSeparation: roundBracketOpen variable (COMMA variable)* roundBracketClose (ASSIGN | PLUS_ASSIGN | MINUS_ASSIGN | MULTIPLY_ASSIGN | DIVIDE_ASSIGN | MODULO_ASSIGN) (tupleExpression | methodCall);

tupleExpression: roundBracketOpen expression (COMMA expression)* roundBracketClose;

expression
    : binaryOp
    | mathOp
    | logicOp
    | variable
    | int
    | float
    | string
    | methodCall
    ;

mathOp: expression mathOpType expression;

mathOpType: PLUS | MINUS | MULTIPLY | DIVIDE | MODULO;

logicOp: expression logicOpType expression;

logicOpType: AND | OR | EQUAL | NOT_EQUAL | NOT | GREATER | LESS | GREATER_EQUAL | LESS_EQUAL;

binaryOp: expression binaryOpType expression;

binaryOpType: AND_BINARY | OR_BINARY | XOR_BINARY;

int: intValue;

intValue: intDigit intValue | intDigit;

intDigit: NUMBER;

float: floatValue;

floatValue: floatDigit floatValue | floatDigit;

floatDigit: NUMBER | DOT;

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

PIPE: '|';
NEWLINE: ('\r'? '\n');
SEMICOLON: ';';

WHITESPACE : (' ' | '\t')+ -> skip;