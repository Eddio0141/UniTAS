using System;
using Sprache;
using UniTASPlugin.Movie.Models.Properties;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.DefaultParsers;

public class DefaultMoviePropertiesParser : IMoviePropertyParser
{
    private static Parser<string> KeyValueLine(string field)
    {
        return
        from key in Sprache.Parse.String(field)
        from space in Sprache.Parse.WhiteSpace.AtLeastOnce()
        from value in Sprache.Parse.AnyChar.Until(Sprache.Parse.LineTerminator).Text().Token()
        select value;
    }

    private static readonly Parser<string> MovieVersion = KeyValueLine("version");
    private static readonly Parser<string> Name = KeyValueLine("name");
    private static readonly Parser<string> Description = KeyValueLine("desc");

    public PropertiesModel Parse(string input)
    {
        // name and desc can be in any order and is null if not present
        return
            from version in MovieVersion
            from name in Name.Optional()
            from desc in Description.Optional()
            select new PropertiesModel(version);
    }
}