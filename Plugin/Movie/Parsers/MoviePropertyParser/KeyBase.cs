namespace UniTASPlugin.Movie.MovieRunner.Parsers.MoviePropertyParser;

public partial class MoviePropertyParser
{
    private abstract class Key
    {
        public string Name { get; }

        // alternative names will be removed from the "to be processed" list when this key is matched
        public string[] AlternativeNames { get; }

        // conflict names will be added to the "conflict" list when this key is matched
        public string[] ConflictKeys { get; }

        protected Key(string name, string[] alternativeNames, string[] conflictKeys)
        {
            Name = name;
            AlternativeNames = alternativeNames;
            ConflictKeys = conflictKeys;
        }

        protected Key(string name) : this(name, new string[0], new string[0])
        {
        }

        public abstract object Parse(string input);
    }
}