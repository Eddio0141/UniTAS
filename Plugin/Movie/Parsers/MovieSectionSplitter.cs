using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.Parsers;

public class MovieSectionSplitter : IMovieSectionSplitter
{
    public IEnumerable<string> Split(string input)
    {
        const string sectionSplitLine = "---";
        var sections = input.Split(new[] { sectionSplitLine }, StringSplitOptions.None);
        return sections;
    }
}