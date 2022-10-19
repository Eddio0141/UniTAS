using System.Collections.Generic;

namespace UniTASPlugin.Movie.ParseInterfaces;

public interface IMovieSectionSplitter
{
    IEnumerable<string> Split(string input);
}