/*
BNF reference

<string_identifier> = <UTF-8 string>

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
<string_value> = <string_identifier> | <escape_char>
<escape_char> = "\" <escape_char_value>
<escape_char_value> = "n" | "t" | "r" | "0" | "\"" | "\\" 
<if_else> = "if" <round_bracket_open> <expression> <round_bracket_close> <scope_open> <program> <scope_close> [ "else" <scope_open> <program> <scope_close> ]
<method_call> = <method_name> <round_bracket_open> <method_call_args> <round_bracket_close>
<method_call_args> = <expression> <method_call_args_separator> <method_call_args> | <expression>
<method_call_args_separator> = ","
<method_def> = "fn" <method_name> <round_bracket_open> <method_def_args> <round_bracket_close> <scope_open> <program> <scope_close>
<method_name> = "a" .. "z" | "A" .. "Z" | "0" .. "9" | "_" | "-"
<method_def_args> = <string_identifier> <method_def_args_separator> <method_def_args> | <string_identifier>
<method_def_args_separator> = ","
<scope_open> = "{"
<scope_close> = "}"
<loop> = "loop" <round_bracket_open> <expression> <round_bracket_close> <scope_open> <program> <scope_close>
<comment> = "//" <string_identifier> | "/*" <string_identifier> "*/"/*
<round_bracket_open> = "("
<round_bracket_close> = ")"
<square_bracket_open> = "["
<square_bracket_close> = "]"
*/

grammar MovieScriptDefaultGrammer;

/*
 * Parser rules
 */

program: action_with_separator action_separator program
        | action_with_separator program
        | action program
        | action;

action_separator: PIPE
                 | NEWLINE
                 | frame_advance;