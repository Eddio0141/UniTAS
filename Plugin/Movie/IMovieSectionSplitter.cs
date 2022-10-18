using System.Collections.Generic;

namespace UniTASPlugin.Movie;

public interface IMovieSectionSplitter
{
    IEnumerable<string> Split(string input);
}