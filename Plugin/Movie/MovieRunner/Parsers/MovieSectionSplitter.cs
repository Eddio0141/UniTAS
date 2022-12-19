using System;
using System.Collections.Generic;
using UniTASPlugin.Movie.MovieRunner.ParseInterfaces;

namespace UniTASPlugin.Movie.MovieRunner.Parsers;

public class DefaultMovieSectionSplitter : IMovieSectionSplitter
{
    public IEnumerable<string> Split(string input)
    {
        const string sectionSplitLine = "---";
        var sections = input.Split(new[] { sectionSplitLine }, StringSplitOptions.None);
        return sections;
    }
}