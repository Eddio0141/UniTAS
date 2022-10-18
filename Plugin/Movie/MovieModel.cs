using System.Linq;
using UniTASPlugin.Movie.Exceptions;
using UniTASPlugin.Movie.Properties;
using UniTASPlugin.Movie.Script;

namespace UniTASPlugin.Movie;

public class MovieModel
{
    public PropertiesModel Properties { get; private set; }
    public ScriptModel Script { get; }

    public MovieModel ParseFromText(string input, IMovieSectionSplitter sectionSplitter, IMoviePropertyParser propertyParser, IMovieScriptParser scriptParser)
    {
        var splitSections = sectionSplitter.Split(input);
        switch (splitSections.Count())
        {
            case 0:
                throw new MissingMoviePropertiesException();
            case 1:
                throw new MissingMovieScriptException();
        }

        Properties = propertyParser.Parse(splitSections.ElementAt(0));
    }
}