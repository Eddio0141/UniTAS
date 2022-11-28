using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.ScriptEngine.Models;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Script;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers;

public class ScriptEngineMovieParser : IMovieParser
{
    private readonly IMovieSectionSplitter _sectionSplitter;
    private readonly IMoviePropertyParser _propertyParser;
    private readonly IMovieScriptParser _scriptParser;

    public ScriptEngineMovieParser(IMovieSectionSplitter sectionSplitter, IMoviePropertyParser propertyParser, IMovieScriptParser scriptParser)
    {
        _sectionSplitter = sectionSplitter;
        _propertyParser = propertyParser;
        _scriptParser = scriptParser;
    }

    public MovieModel Parse(string input)
    {
        var splitSections = _sectionSplitter.Split(input).ToList();
        switch (splitSections.Count())
        {
            case 0:
                throw new MissingMoviePropertiesException();
            case 1:
                throw new MissingMovieScriptException();
        }

        var properties = _propertyParser.Parse(splitSections.ElementAt(0));
        var methods = _scriptParser.Parse(splitSections.ElementAt(1));

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