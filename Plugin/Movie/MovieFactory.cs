using System.Linq;
using UniTASPlugin.Movie.DefaultParser;
using UniTASPlugin.Movie.Exceptions;
using UniTASPlugin.Movie.Script;

namespace UniTASPlugin.Movie;

public class MovieFactory
{
    public static MovieModel ParseFromText(string input)
    {
        var sectionSplitter = new DefaultMovieSectionSplitter();
        var propertyParser = new DefaultMoviePropertiesParser();

        var splitSections = sectionSplitter.Split(input).ToList();
        switch (splitSections.Count())
        {
            case 0:
                throw new MissingMoviePropertiesException();
            case 1:
                throw new MissingMovieScriptException();
        }

        var properties = propertyParser.Parse(splitSections.ElementAt(0));
        var script = ScriptFactory.ParseFromText(splitSections.ElementAt(1));

        return new MovieModel(properties, script);
    }
}