using System.Collections.Generic;

namespace UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

public interface IMovieSectionSplitter
{
    IEnumerable<string> Split(string input);
}