namespace UniTASPlugin.Movie.DefaultParsers.DefaultMoviePropertiesParser;

public partial class DefaultMoviePropertiesParser
{
    private class VersionKey : KeyBase
    {
        public const string Key = "version";

        public VersionKey() : base(Key)
        {
        }

        public override object Parse(string input)
        {
            return input;
        }
    }
}