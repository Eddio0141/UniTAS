using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.DefaultParsers;

public class DefaultMovieSectionSplitter : IMovieSectionSplitter
{
    public IEnumerable<string> Split(string input)
    {
        var sectionSplitLine = "---";
        var sections = input.Split(new[] { sectionSplitLine }, StringSplitOptions.None);
        return sections;
    }
}