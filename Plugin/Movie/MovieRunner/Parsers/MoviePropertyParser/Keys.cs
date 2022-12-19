using System;
using System.Collections.Generic;
using System.Globalization;
using UniTASPlugin.GameEnvironment.InnerState;
using UniTASPlugin.Movie.ScriptEngine.Exceptions.ParseExceptions;

// ReSharper disable StringLiteralTypo

namespace UniTASPlugin.Movie.ScriptEngine.Parsers.MoviePropertiesParser;

public partial class MoviePropertyParser
{
    private class OsKey : Key
    {
        public const string Key = "os";

        public OsKey() : base(Key, new string[0], new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => Enum.Parse(typeof(Os), input);
    }

    private class StartTimeKey : Key
    {
        public const string Key = "datetime";

        public StartTimeKey() : base(Key, new[] { SeedKey.Key }, new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input)
        {
            return DateTime.Parse(input, CultureInfo.InvariantCulture,
                DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AdjustToUniversal);
        }
    }

    private class SeedKey : Key
    {
        public const string Key = "seed";

        public SeedKey() : base(Key, new[] { StartTimeKey.Key }, new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => new DateTime(long.Parse(input));
    }

    private class FrameTimeKey : Key
    {
        public const string Key = "frametime";

        public FrameTimeKey() : base(Key, new[] { FtKey.Key, FpsKey.Key }, new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => float.Parse(input);
    }

    private class FtKey : Key
    {
        public const string Key = "ft";

        public FtKey() : base(Key, new[] { FrameTimeKey.Key, FpsKey.Key }, new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => float.Parse(input);
    }

    private class FpsKey : Key
    {
        public const string Key = "fps";

        public FpsKey() : base(Key, new[] { FtKey.Key, FpsKey.Key }, new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => 1f / float.Parse(input);
    }

    private class ResolutionKey : Key
    {
        public const string Key = "resolution";

        public ResolutionKey() : base(Key, new string[0], new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input)
        {
            // width height
            var split = input.Split(new[] { ' ' }, 2, StringSplitOptions.None);
            var widthString = split.Length > 0 ? split[0] : throw new MissingResolutionFieldException("width");
            var heightString = split.Length > 1 ? split[1] : throw new MissingResolutionFieldException("height");

            var width = (int)uint.Parse(widthString);
            var height = (int)uint.Parse(heightString);

            return new KeyValuePair<int, int>(width, height);
        }
    }

    private class Unfocused : Key
    {
        public const string Key = "unfocused";

        public Unfocused() : base(Key, new string[0], new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => false;
    }

    private class FullScreen : Key
    {
        public const string Key = "fullscreen";

        public FullScreen() : base(Key, new string[0], new[] { FromSaveState.Key })
        {
        }

        public override object Parse(string input) => true;
    }

    private class FromSaveState : Key
    {
        public const string Key = "from_savestate";

        public FromSaveState() : base(Key, new string[0], new[]
        {
            OsKey.Key,
            StartTimeKey.Key,
            SeedKey.Key,
            FrameTimeKey.Key,
            FtKey.Key,
            FpsKey.Key,
            ResolutionKey.Key,
            Unfocused.Key,
            FullScreen.Key
        })
        {
        }

        public override object Parse(string input) => input;
    }

    private class NameKey : Key
    {
        public const string Key = "name";

        public NameKey() : base(Key)
        {
        }

        public override object Parse(string input) => input;
    }

    private class DescriptionKey : Key
    {
        public const string Key = "desc";

        public DescriptionKey() : base(Key)
        {
        }

        public override object Parse(string input) => input;
    }

    private class AuthorKey : Key
    {
        public const string Key = "author";

        public AuthorKey() : base(Key)
        {
        }

        public override object Parse(string input) => input;
    }

    private class EndSaveKey : Key
    {
        public const string Key = "endsave";

        public EndSaveKey() : base(Key)
        {
        }

        public override object Parse(string input) => input;
    }
}