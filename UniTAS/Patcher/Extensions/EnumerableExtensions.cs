using System.Collections.Generic;

namespace UniTAS.Patcher.Extensions;

public static class EnumerableExtensions
{
    public static ulong Average(this IEnumerable<ulong> source)
    {
        var count = 0ul;
        var total = 0ul;
        foreach (var value in source)
        {
            total += value;
            count++;
        }

        if (count == 0) return 0ul;

        return total / count;
    }

    public static void AddRange<T>(this HashSet<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }
}