using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UniTAS.Patcher.Utils;

public static class HashUtils
{
    public class ReferenceComparer<T> : IEqualityComparer<T>
    {
        bool IEqualityComparer<T>.Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}