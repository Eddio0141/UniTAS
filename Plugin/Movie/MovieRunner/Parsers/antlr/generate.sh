#!/bin/bash
antlr4 -v 4.9.3 -Dlanguage=CSharp MovieScriptDefaultGrammar.g4

# in the generated .cs files, remove [System.CLSCompliant(false)] from the class declaration
for f in *.cs; do
    sed -i 's/\[System\.CLSCompliant(false)\]//' $f
done