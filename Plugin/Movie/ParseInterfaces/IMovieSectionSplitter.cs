using System.Collections.Generic;

namespace UniTASPlugin.Movie.MovieRunner.ParseInterfaces;

public interface IMovieSectionSplitter
{
    IEnumerable<string> Split(string input);
}