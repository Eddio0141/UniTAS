using System.Collections.Generic;

namespace UniTAS.Plugin.Movie.ParseInterfaces;

public interface IMovieSectionSplitter
{
    IEnumerable<string> Split(string input);
}