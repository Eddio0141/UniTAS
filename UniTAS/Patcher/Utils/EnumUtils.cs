using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UniTAS.Patcher.Utils;

public static class EnumUtils
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, object>> EnumNamesCache = new();

    public static bool TryParse<T>(string valueToParse, out T returnValue, bool ignoreCase = false)
        where T : struct
    {
        if (string.IsNullOrEmpty(valueToParse)) throw new ArgumentNullException(nameof(valueToParse));
        
        if (!ignoreCase)
        {
            // easier to handle this
            if (Enum.IsDefined(typeof(T), valueToParse))
            {
                returnValue = (T)Enum.Parse(typeof(T), valueToParse, false);
                return true;
            }

            returnValue = default;
            return false;
        }

        // generate matching lowercase names and values
        var enumNames =
            EnumNamesCache.GetOrAdd(typeof(T), t =>
            {
                var values = Enum.GetValues(t);
                return Enum.GetNames(t)
                    .Select((x, i) => (x.ToLowerInvariant(), values.GetValue(i)))
                    .ToDictionary(s => s.Item1, s => s.Item2);
            });

        if (enumNames.TryGetValue(valueToParse.ToLowerInvariant(), out var returnValueRaw))
        {
            returnValue = (T)returnValueRaw;
            return true;
        }

        returnValue = default;
        return false;
    }
}