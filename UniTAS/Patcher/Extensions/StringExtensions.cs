using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UniTAS.Patcher.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Compares the string against a given pattern.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
    /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
    public static bool Like(this string str, string pattern)
    {
        return new Regex(
            "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline
        ).IsMatch(str);
    }

    public static int[] AllIndexesOfAny(this string str, char[] chars, int startIndex = 0)
    {
        var lastFound = str.IndexOfAny(chars, startIndex);
        if (lastFound == -1) return [];

        var indexes = new List<int> { lastFound };
        while (true)
        {
            lastFound = str.IndexOfAny(chars, lastFound + 1);
            if (lastFound == -1) return indexes.ToArray();
            indexes.Add(lastFound);
        }
    }

    public static string[] SplitByUpperCase(this string str)
    {
        var result = new List<string>();
        var lastFound = 0;
        for (var i = 1; i < str.Length; i++)
        {
            var c = str[i];
            if (!char.IsUpper(c)) continue;
            var chunk = str.Substring(lastFound, i - lastFound);
            result.Add(chunk);
            lastFound = i;
        }
        
        if (lastFound < str.Length - 1)
            result.Add(str.Substring(lastFound));

        return result.ToArray();
    }
}