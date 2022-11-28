using System;
using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;
using UniTASPlugin.Movie.ScriptEngine.Models.Movie.Properties;
using UniTASPlugin.Movie.ScriptEngine.ParseInterfaces;

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;

public partial class DefaultMoviePropertiesParser : IMoviePropertyParser
{
    public PropertiesModel Parse(string input)
    {
        var lines = input.Split('\n');
        var leftKeys = new List<KeyBase>()
        {
            new OsKey(),
            new StartTimeKey(),
            new SeedKey(),
            new FrameTimeKey(),
            new FtKey(),
            new FpsKey(),
            new ResolutionKey(),
            new Unfocused(),
            new FullScreen(),
            new FromSaveState(),
            new NameKey(),
            new DescriptionKey(),
            new AuthorKey(),
            new EndSaveKey()
        };
        // key is the conflicting keys, value is what key caused the conflict
        var conflictKeys = new List<KeyValuePair<string, string>>();
        // for a better error message
        var processedKeys = new List<KeyBase>();

        string name = null;
        string description = null;
        string author = null;

        Os? os = null;
        DateTime? startTime = null;
        float? frameTime = null;

        // WindowState
        KeyValuePair<int, int>? resolution = null;
        bool? isFullScreen = null;
        bool? isFocused = null;

        string endSavePath = null;
        string loadSaveStatePath = null;

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
                var dupeKeyIndex = processedKeys.FindIndex(x => x.Name == key);
                var altKeyIndex = processedKeys.FindIndex(x => x.AlternativeNames.Contains(key));
                if (dupeKeyIndex < 0 && altKeyIndex < 0)
                {
                    throw new InvalidPropertyKeyException(key);
                    // this means the key is invalid
                }
                if (altKeyIndex < 0)
                {
                    // this means the key is a dupe
                    var dupeKey = processedKeys[dupeKeyIndex];
                    if (dupeKey.Name != key)
                    {
                        // key was already processed through alternative key
                        throw new DuplicatePropertyKeyException(dupeKey.Name);
                    }
                }
                throw new DuplicatePropertyKeyException();
            }

            var foundKey = leftKeys[foundKeyIndex];
            var foundConflictKeyIndex = conflictKeys.FindIndex(x => x.Key == foundKey.Name);
            if (foundConflictKeyIndex > -1)
                throw new ConflictingPropertyKeyException(foundKey.Name, conflictKeys[foundConflictKeyIndex].Value);

            leftKeys.RemoveAt(foundKeyIndex);
            foreach (var altKey in foundKey.AlternativeNames)
            {
                var altKeyIndex = leftKeys.FindIndex(x => x.Name == altKey);
                leftKeys.RemoveAt(altKeyIndex);
            }
            foreach (var conflictKey in foundKey.ConflictKeys)
            {
                conflictKeys.Add(new KeyValuePair<string, string>(conflictKey, foundKey.Name));
            }

            var parsedValue = foundKey.Parse(split.Length < 2 ? "" : split[1].TrimStart());
            switch (foundKey.Name)
            {
                case OsKey.Key:
                    os = (Os?)parsedValue;
                    break;
                case StartTimeKey.Key:
                    startTime = (DateTime?)parsedValue;
                    break;
                case SeedKey.Key:
                    startTime = (DateTime?)parsedValue;
                    break;
                case FrameTimeKey.Key:
                case FtKey.Key:
                    frameTime = (float?)parsedValue;
                    break;
                case FpsKey.Key:
                    frameTime = (float?)parsedValue;
                    break;
                case ResolutionKey.Key:
                    resolution = (KeyValuePair<int, int>?)parsedValue;
                    break;
                case Unfocused.Key:
                    isFocused = (bool?)parsedValue;
                    break;
                case FullScreen.Key:
                    isFullScreen = (bool?)parsedValue;
                    break;
                case FromSaveState.Key:
                    loadSaveStatePath = (string)parsedValue;
                    break;
                case NameKey.Key:
                    name = (string)parsedValue;
                    break;
                case DescriptionKey.Key:
                    description = (string)parsedValue;
                    break;
                case AuthorKey.Key:
                    author = (string)parsedValue;
                    break;
                case EndSaveKey.Key:
                    endSavePath = (string)parsedValue;
                    break;
                default:
                    throw new InvalidOperationException("Unreachable code");
            }

            processedKeys.Add(foundKey);
        }

        if (loadSaveStatePath != null) return new PropertiesModel(name, description, author, endSavePath, loadSaveStatePath);
        if (resolution == null)
            throw new UnknownMovieStartOptionException();

        var windowState = new WindowState(resolution.Value.Key, resolution.Value.Value, isFullScreen.Value, isFocused.Value);
        var startupProperties = new StartupPropertiesModel(os.Value, startTime.Value, frameTime.Value, windowState);
        return new PropertiesModel(name, description, author, endSavePath, startupProperties);
    }
}
