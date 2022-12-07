antlr4 -v 4.9.3 -Dlanguage=CSharp MovieScriptDefaultGrammar.g4

# in the generated .cs files, remove [System.CLSCompliant(false)] from the class declaration
foreach ($file in Get-ChildItem -Path .\MovieScriptDefaultGrammar*.cs) {
    $content = Get-Content $file.FullName
    $content = $content -replace '\[System\.CLSCompliant\(false\)\]', ''
    Set-Content $file.FullName $content
}