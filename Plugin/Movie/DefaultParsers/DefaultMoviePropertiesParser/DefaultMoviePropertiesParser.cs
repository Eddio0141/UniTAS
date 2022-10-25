using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.Movie.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.Models.Properties;
using UniTASPlugin.Movie.ParseInterfaces;

namespace UniTASPlugin.Movie.DefaultParsers.DefaultMoviePropertiesParser;

public partial class DefaultMoviePropertiesParser : IMoviePropertyParser
{
    public PropertiesModel Parse(string input)
    {
        var lines = input.Split('\n');
        var leftKeys = new List<KeyBase>()
        {
            new VersionKey(),
        };
        var conflictKeys = new List<KeyBase>();
        // for a better error message
        var processedKeys = new List<KeyBase>();

        string movieVersion;
        string name;
        string description;
        string author;
        StartupPropertiesModel startupProperties;
        string endSavePath;

        foreach (var line in lines)
        {
            var lineTrim = line.Trim();
            var split = lineTrim.Split(new[] { ' ' }, 2, StringSplitOptions.None);

            if (split.Length == 0)
                continue;

            var key = split[0].Trim();
            var foundKeyIndex = leftKeys.FindIndex(x => x.Name == key);

            if (foundKeyIndex < 0)
            {
                var dupeKeyIndex = processedKeys.FindIndex(x => x.Name == key || x.AlternativeNames.Contains(key));
                if (dupeKeyIndex < 0)
                {
                    // this means the key is invalid
                    throw new InvalidPropertyKeyException();
                }
                // this means the key is a dupe
                var dupeKey = processedKeys[dupeKeyIndex];
                if (dupeKey.Name != key)
                {
                    // key was already processed through alternative key
                    throw new DuplicatePropertyKeyException(dupeKey.Name);
                }
                throw new DuplicatePropertyKeyException();
            }

            var foundKey = leftKeys[foundKeyIndex];
            leftKeys.RemoveAt(foundKeyIndex);

            var parsedValue = foundKey.Parse(split.Length == 2 ? null : split[1]);
            switch (foundKey.Name)
            {
                case VersionKey.Key:
                    {
                        movieVersion = (string)parsedValue;
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unreachable code");
            }

            processedKeys.Add(foundKey);
        }

        var versionKeyName = new VersionKey().Name;
        if (leftKeys.Exists(x => x.Name == versionKeyName))
            throw new MissingMovieVersionException();

        switch (movieStartOption)
        {
            default:
                throw new MissingMovieStartOption();
        }
        return new PropertiesModel(movieVersion, name, description, author)
    }
}
