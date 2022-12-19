using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers;

public class DefaultMovieSectionSplitter : IMovieSectionSplitter
{
    public IEnumerable<string> Split(string input)
    {
        const string sectionSplitLine = "---";
        var sections = input.Split(new[] { sectionSplitLine }, StringSplitOptions.None);
        return sections;
    }
}