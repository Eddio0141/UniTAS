using System;
using System.Collections.Generic;

namespace UniTAS.Plugin.Extensions;

public static class ListExtensions
{
    public static void RemoveAllEquals<T>(this List<T> list, T item)
        where T : IEquatable<T>
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (!list[i].Equals(item)) continue;
            list.RemoveAt(i);
            i--;
        }
    }
}