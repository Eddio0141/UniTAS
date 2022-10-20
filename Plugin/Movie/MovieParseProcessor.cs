using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UniTASPlugin.Movie.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.Models;
using UniTASPlugin.Movie.Models.Script;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie;

public class MovieParseProcessor
{
    public static MovieModel Parse(string input, IMovieSectionSplitter sectionSplitter, IMoviePropertyParser propertyParser, IMovieScriptParser scriptParser)
    {
        var splitSections = sectionSplitter.Split(input).ToList();
        switch (splitSections.Count())
        {
            case 0:
                throw new MissingMoviePropertiesException();
            case 1:
                throw new MissingMovieScriptException();
        }

        var properties = propertyParser.Parse(splitSections.ElementAt(0));
        var methods = scriptParser.Parse(splitSections.ElementAt(1));

        var mainMethod = new ScriptMethodModel();
        var definedMethods = new List<ScriptMethodModel>();

        foreach (var method in methods)
        {
            if (method.Name.IsNullOrWhiteSpace())
            {
                mainMethod = method;
            }
            else if (definedMethods.FindIndex(s => s.Name == method.Name) > -1)
            {
                throw new DuplicateMethodsDefinedException();
            }
            else
            {
                definedMethods.Add(method);
            }
        }
        var script = new ScriptModel(mainMethod, definedMethods);

        return new MovieModel(properties, script);
    }
}